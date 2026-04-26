using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IReadOnlyInventory
{
    string UniqueId { get; }
    string InventoryName { get; }
    int SlotCount { get; }
    int UsedSlotCount { get; }
    int TotalItemCount { get; }
    IReadOnlyList<InventorySlot> Slots { get; }
    bool IsInitialized { get; }
    bool CanAddItem(Item item, int amount = 1);
    int GetItemCount(string itemGuid);
    bool HasItem(string itemGuid, int amount = 1);
    InventoryData GetInventoryData();
}

public interface IInventory : IReadOnlyInventory
{
    event Action<Inventory> Changed;
    event Action<InventorySlot> SlotChanged;

    InventoryOperationResult AddItem(Item item, int amount);
    InventoryOperationResult RemoveItems(string itemGuid, int amount);
    InventoryOperationResult TransferSlotTo(InventorySlot sourceSlot, Inventory targetInventory);
    InventoryOperationResult TransferAllTo(Inventory targetInventory);
    bool TryMoveOrSwap(InventorySlot sourceSlot, InventorySlot destinationSlot);
    bool TrySplitSlot(InventorySlot slot, out InventorySlot createdSlot);
    void Clear();
}

public readonly struct InventoryOperationResult
{
    public int RequestedAmount { get; }
    public int ProcessedAmount { get; }
    public int RemainingAmount { get; }
    public bool Succeeded => ProcessedAmount > 0;
    public bool FullyProcessed => RemainingAmount <= 0;

    public InventoryOperationResult(int requestedAmount, int processedAmount)
    {
        RequestedAmount = Mathf.Max(0, requestedAmount);
        ProcessedAmount = Mathf.Clamp(processedAmount, 0, RequestedAmount);
        RemainingAmount = RequestedAmount - ProcessedAmount;
    }

    public static InventoryOperationResult None(int requestedAmount)
    {
        return new InventoryOperationResult(requestedAmount, 0);
    }
}

[DisallowMultipleComponent]
public class Inventory : SlotContainer<Item>, IInventory
{
    #region Constants

    private const int DefaultSlotCount = 20;

    #endregion

    #region Events

    public event Action<Inventory> Changed;
    public event Action<InventorySlot> SlotChanged;

    #endregion

    #region Inspector Fields

    [Header("Inventory Settings")]
    [Tooltip("Stable id written into InventoryData. Generated automatically when empty.")]
    [SerializeField] protected string uniqueId;

    [Tooltip("Name displayed in the inventory UI title, if the template has one.")]
    [SerializeField] protected string inventoryName = "Inventory";

    [Tooltip("Optional root UI Toolkit template for this inventory.")]
    [SerializeField] protected VisualTreeAsset layoutTemplate;

    [Tooltip("Optional slot template cloned into the layout template.")]
    [SerializeField] protected VisualTreeAsset slotTemplate;

    [Tooltip("Number of slots to create when using a slot template or headless inventory.")]
    [SerializeField] protected int slotCount;

    [Header("Filters")]
    [Tooltip("All listed filters must accept an item before it can be added.")]
    [SerializeField] protected SO_ItemFilter[] filters = new SO_ItemFilter[0];

    [Header("Sorting")]
    [Tooltip("Optional rarity list that defines rarity names, colors, backgrounds and sort order.")]
    [SerializeField] protected SO_RarityCatalog rarityCatalog;

    [Tooltip("First comparison used by CompactAndSort.")]
    [SerializeField] protected InventorySortKey primarySort = InventorySortKey.Rarity;

    [Tooltip("Second comparison used when the primary sort is equal.")]
    [SerializeField] protected InventorySortKey secondarySort = InventorySortKey.Name;

    [Tooltip("When enabled, higher rarity sort values appear first.")]
    [SerializeField] protected bool sortRarityDescending = true;

    #endregion

    #region Runtime Fields

    protected VisualElement inventoryRoot;
    protected VisualElement slotContainer;
    protected Button sortButton;

    protected bool isInitialized;
    protected bool isUIInitialized;

    #endregion

    #region Properties

    /// <summary>Stable id used by save data to match this inventory across sessions.</summary>
    public string UniqueId => uniqueId;

    /// <summary>Display name shown by the default UI template.</summary>
    public string InventoryName => inventoryName;

    /// <summary>Runtime slots owned by this inventory.</summary>
    public InventorySlot[] ItemSlots
    {
        get
        {
            EnsureInitialized();
            return (InventorySlot[])slots;
        }
    }

    public IReadOnlyList<InventorySlot> Slots => ItemSlots;
    public int SlotCount => ItemSlots.Length;
    public int UsedSlotCount => CountUsedSlots();
    public int TotalItemCount => CountItems();

    public VisualElement InventoryRoot => inventoryRoot;
    public bool IsInitialized => isInitialized;
    public SO_RarityCatalog RarityCatalog => rarityCatalog;

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        EnsureUniqueId();

        if (layoutTemplate == null)
        {
            if (slotCount > 0)
            {
                containerSize = slotCount;
            }
            else if (containerSize <= 0)
            {
                containerSize = DefaultSlotCount;
            }

            InitializeInventory();
            return;
        }

        QueryUIReferences();
        InitializeInventory();
    }

    protected virtual void OnValidate()
    {
        EnsureUniqueId();

        slotCount = Mathf.Max(0, slotCount);
    }

    protected virtual void OnDestroy()
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.UnregisterInventoryUI(inventoryRoot);
        }
    }

    #endregion

    #region Queries

    public bool CanAddItem(Item item, int amount = 1)
    {
        EnsureInitialized();

        if (item == null || amount <= 0 || !PassesInventoryFilters(item))
        {
            return false;
        }

        int remaining = amount;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (slot.CanStack(item) && slot.IsItemAllowed(item))
            {
                remaining -= item.StackSize - slot.Amount;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        foreach (InventorySlot slot in ItemSlots)
        {
            if (slot.IsEmpty && slot.IsItemAllowed(item))
            {
                remaining -= item.StackSize;

                if (remaining <= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetItemCount(string itemGuid)
    {
        int count = 0;
        EnsureInitialized();

        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty && slot.Item != null && slot.Item.Guid == itemGuid)
            {
                count += slot.Amount;
            }
        }

        return count;
    }

    public bool HasItem(string itemGuid, int amount)
    {
        return GetItemCount(itemGuid) >= amount;
    }

    public InventorySlot GetSlot(int index)
    {
        if (TryGetSlot(index, out InventorySlot slot))
        {
            return slot;
        }

        return null;
    }

    public bool TryGetSlot(int index, out InventorySlot slot)
    {
        EnsureInitialized();

        if (index < 0 || index >= ItemSlots.Length)
        {
            slot = null;
            return false;
        }

        slot = ItemSlots[index];
        return true;
    }

    #endregion

    #region Item Operations

    /// <summary>
    /// Adds as many items as possible and returns the amount that did not fit.
    /// </summary>
    public virtual int TryAddItem(Item item, int amount)
    {
        return AddItem(item, amount).RemainingAmount;
    }

    public virtual InventoryOperationResult AddItem(Item item, int amount)
    {
        EnsureInitialized();

        if (item == null || amount <= 0)
        {
            return InventoryOperationResult.None(amount);
        }

        if (!PassesInventoryFilters(item))
        {
            return InventoryOperationResult.None(amount);
        }

        int remaining = amount;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (slot.CanStack(item) && slot.IsItemAllowed(item))
            {
                int space = item.StackSize - slot.Amount;
                int toAdd = Mathf.Min(space, remaining);
                slot.UpdateSlot(item, slot.Amount + toAdd);
                remaining -= toAdd;
            }
        }

        foreach (InventorySlot slot in ItemSlots)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (slot.IsEmpty && slot.IsItemAllowed(item))
            {
                int toAdd = Mathf.Min(item.StackSize, remaining);
                slot.UpdateSlot(item, toAdd);
                remaining -= toAdd;
            }
        }

        return new InventoryOperationResult(amount, amount - remaining);
    }

    /// <summary>
    /// Removes items by item guid and returns the amount that could not be removed.
    /// </summary>
    public int RemoveItem(string itemGuid, int amount)
    {
        return RemoveItems(itemGuid, amount).RemainingAmount;
    }

    public InventoryOperationResult RemoveItems(string itemGuid, int amount)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(itemGuid) || amount <= 0)
        {
            return InventoryOperationResult.None(amount);
        }

        int remaining = amount;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (!slot.IsEmpty && slot.Item?.Guid == itemGuid)
            {
                int remove = Mathf.Min(slot.Amount, remaining);
                slot.UpdateSlot(slot.Item, slot.Amount - remove);
                remaining -= remove;
            }
        }

        return new InventoryOperationResult(amount, amount - remaining);
    }

    public void Clear()
    {
        EnsureInitialized();
        ClearAllSlots();
    }

    #endregion

    #region Slot Operations

    public void SplitSlotItem(InventorySlot slot)
    {
        TrySplitSlot(slot, out _);
    }

    public bool TrySplitSlot(InventorySlot slot, out InventorySlot createdSlot)
    {
        createdSlot = null;

        if (slot == null || slot.Parent != this || slot.Amount <= 1)
        {
            return false;
        }

        InventorySlot freeSlot = GetFreeInventorySlot(slot.Item);

        if (freeSlot == null || !freeSlot.IsItemAllowed(slot.Item))
        {
            return false;
        }

        int half = slot.Amount / 2;
        slot.UpdateSlot(slot.Item, slot.Amount - half);
        freeSlot.UpdateSlot(slot.Item, half);
        createdSlot = freeSlot;
        return true;
    }

    public static void TransferItem(InventorySlot sourceSlot, Inventory targetInventory)
    {
        TransferSlot(sourceSlot, targetInventory);
    }

    public InventoryOperationResult TransferSlotTo(InventorySlot sourceSlot, Inventory targetInventory)
    {
        return TransferSlot(sourceSlot, targetInventory);
    }

    public static InventoryOperationResult TransferSlot(InventorySlot sourceSlot, Inventory targetInventory)
    {
        if (sourceSlot == null || sourceSlot.IsEmpty || targetInventory == null)
        {
            return InventoryOperationResult.None(0);
        }

        Item sourceItem = sourceSlot.Item;
        int sourceAmount = sourceSlot.Amount;
        InventoryOperationResult result = targetInventory.AddItem(sourceItem, sourceAmount);
        sourceSlot.UpdateSlot(sourceItem, result.RemainingAmount);
        return result;
    }

    public InventoryOperationResult TransferAllTo(Inventory targetInventory)
    {
        EnsureInitialized();

        if (targetInventory == null || targetInventory == this)
        {
            return InventoryOperationResult.None(0);
        }

        int requested = 0;
        int processed = 0;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            requested += slot.Amount;
            InventoryOperationResult result = TransferSlot(slot, targetInventory);
            processed += result.ProcessedAmount;
        }

        return new InventoryOperationResult(requested, processed);
    }

    public bool TryMoveOrSwap(InventorySlot sourceSlot, InventorySlot destinationSlot)
    {
        return MoveOrSwapSlots(sourceSlot, destinationSlot).Succeeded;
    }

    public static InventoryOperationResult MoveOrSwapSlots(InventorySlot sourceSlot, InventorySlot destinationSlot)
    {
        if (sourceSlot == null || destinationSlot == null || sourceSlot == destinationSlot || sourceSlot.IsEmpty)
        {
            return InventoryOperationResult.None(0);
        }

        Item sourceItem = sourceSlot.Item;
        int sourceAmount = sourceSlot.Amount;
        Item destinationItem = destinationSlot.Item;
        int destinationAmount = destinationSlot.Amount;

        if (!destinationSlot.IsEmpty && destinationSlot.CanStack(sourceItem) && destinationSlot.IsItemAllowed(sourceItem))
        {
            int space = destinationItem.StackSize - destinationAmount;
            int moveAmount = Mathf.Min(space, sourceAmount);
            destinationSlot.UpdateSlot(destinationItem, destinationAmount + moveAmount);
            sourceSlot.UpdateSlot(sourceItem, sourceAmount - moveAmount);
            return new InventoryOperationResult(sourceAmount, moveAmount);
        }

        bool destinationAllowsSource = destinationSlot.IsItemAllowed(sourceItem);
        bool sourceAllowsDestination = destinationSlot.IsEmpty || sourceSlot.IsItemAllowed(destinationItem);

        if (destinationAllowsSource && sourceAllowsDestination)
        {
            sourceSlot.UpdateSlot(destinationItem, destinationAmount);
            destinationSlot.UpdateSlot(sourceItem, sourceAmount);
            return new InventoryOperationResult(sourceAmount, sourceAmount);
        }

        return InventoryOperationResult.None(sourceAmount);
    }

    #endregion

    #region Sort

    /// <summary>
    /// Merges matching stacks, clears the inventory, then writes the stacks back using the configured sort keys.
    /// </summary>
    public void CompactAndSort()
    {
        EnsureInitialized();

        var itemStacks = new Dictionary<(string guid, string rarity), (Item item, int amount)>();

        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty && slot.Item != null)
            {
                var key = (slot.Item.Guid, RarityDefinition.NormalizeName(slot.Item.Rarity));
                if (itemStacks.TryGetValue(key, out var entry))
                {
                    itemStacks[key] = (slot.Item, entry.amount + slot.Amount);
                }
                else
                {
                    itemStacks[key] = (slot.Item, slot.Amount);
                }
            }
        }

        var sortedItems = new List<(Item item, int amount)>(itemStacks.Values);
        sortedItems.Sort(CompareItemStacks);

        ClearAllSlots();

        int slotIndex = 0;
        foreach (var (item, amount) in sortedItems)
        {
            int remain = amount;
            while (remain > 0 && slotIndex < ItemSlots.Length)
            {
                int toAdd = Mathf.Min(item.StackSize, remain);
                ItemSlots[slotIndex].UpdateSlot(item, toAdd);
                remain -= toAdd;
                slotIndex++;
            }
        }
    }

    /// <summary>
    /// Changes the rarity catalog used for future sorting operations.
    /// </summary>
    public void SetRarityCatalog(SO_RarityCatalog catalog)
    {
        rarityCatalog = catalog;
    }

    /// <summary>
    /// Changes the item comparison used by CompactAndSort.
    /// </summary>
    public void SetSortOptions(InventorySortKey primary, InventorySortKey secondary, bool rarityDescending = true)
    {
        primarySort = primary;
        secondarySort = secondary;
        sortRarityDescending = rarityDescending;
    }

    #endregion

    #region Serialization

    public virtual void SetInventoryData(InventoryData data)
    {
        if (data == null)
        {
            return;
        }

        EnsureInitialized();

        uniqueId = data.InventoryGuid;
        inventoryName = data.InventoryName;

        for (int i = 0; i < data.InventorySlots.Length && i < ItemSlots.Length; i++)
        {
            InventorySlotData slotData = data.InventorySlots[i];
            if (slotData != null)
            {
                ItemSlots[i].UpdateSlot(slotData.InventoryItem, slotData.Amount);
            }
        }
    }

    public InventoryData GetInventoryData()
    {
        var data = new InventoryData
        {
            InventoryGuid = uniqueId,
            InventoryName = inventoryName,
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles,
            Scale = transform.localScale,
            PrefabName = gameObject.name,
            InventorySlots = new InventorySlotData[containerSize]
        };

        for (int i = 0; i < ItemSlots.Length; i++)
        {
            if (!ItemSlots[i].IsEmpty && ItemSlots[i].Item != null)
            {
                data.InventorySlots[i] = new InventorySlotData
                {
                    InventoryItem = ItemSlots[i].Item,
                    Amount = ItemSlots[i].Amount
                };
            }
        }

        return data;
    }

    #endregion

    #region UI

    public virtual VisualElement GetInventoryUI()
    {
        if (layoutTemplate == null)
        {
            return null;
        }

        if (isInitialized && isUIInitialized && inventoryRoot != null)
        {
            return inventoryRoot;
        }

        EnsureInitialized();
        EnsureUIBuild();

        return inventoryRoot;
    }

    #endregion

    #region Protected Helpers

    protected virtual void QueryUIReferences()
    {
        inventoryRoot = layoutTemplate.CloneTree();
        slotContainer = inventoryRoot.Q<VisualElement>("InventorySlotContainer");
        sortButton = inventoryRoot.Q<Button>("SortButton");

        if (slotContainer == null)
        {
            Debug.LogError($"[{nameof(Inventory)}] Layout template '{layoutTemplate.name}' is missing a VisualElement named 'InventorySlotContainer'.", this);
            containerSize = Mathf.Max(slotCount, DefaultSlotCount);
            return;
        }

        var titleLabel = inventoryRoot.Q<Label>("InventoryTitle");
        if (titleLabel != null && !string.IsNullOrEmpty(inventoryName))
        {
            titleLabel.text = inventoryName;
        }

        if (slotTemplate != null && slotCount > 0)
        {
            slotContainer.Clear();
            for (int i = 0; i < slotCount; i++)
            {
                VisualElement slot = slotTemplate.CloneTree();
                slotContainer.Add(slot);
            }
            containerSize = slotCount;
        }
        else
        {
            containerSize = slotContainer.childCount;
        }
    }

    protected virtual void InitializeInventory()
    {
        InitializeContainer(() => new InventorySlot(this));
    }

    protected override void InitializeContainer(Func<Slot<Item>> slotFactory)
    {
        slots = new InventorySlot[containerSize];

        for (int i = 0; i < containerSize; i++)
        {
            slots[i] = slotFactory();
        }

        isInitialized = true;
    }

    protected virtual void BuildUI()
    {
        if (slotContainer == null)
        {
            return;
        }

        int slotUiCount = Mathf.Min(slotContainer.childCount, ItemSlots.Length);
        for (int i = 0; i < slotUiCount; i++)
        {
            VisualElement child = slotContainer.ElementAt(i);
            ItemSlots[i].InitializeUI(child);
            child.userData = ItemSlots[i];
            child.AddToClassList("inventory-slot");
        }

        LinkCallbacks();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RegisterInventoryUI(inventoryRoot);
        }

        isUIInitialized = true;
    }

    protected virtual void LinkCallbacks()
    {
        if (sortButton != null)
        {
            sortButton.clicked -= CompactAndSort;
            sortButton.clicked += CompactAndSort;
        }
    }

    protected void ClearAllSlots()
    {
        foreach (InventorySlot slot in ItemSlots)
        {
            slot.UpdateSlot(null, 0);
        }
    }

    protected InventorySlot GetFreeInventorySlot()
    {
        return GetFreeInventorySlot(null);
    }

    protected InventorySlot GetFreeInventorySlot(Item item)
    {
        foreach (InventorySlot slot in ItemSlots)
        {
            if (slot.IsEmpty && (item == null || slot.IsItemAllowed(item)))
            {
                return slot;
            }
        }

        return null;
    }

    protected bool PassesInventoryFilters(Item item)
    {
        if (item == null || filters.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i] != null && !filters[i].Filter(item))
            {
                return false;
            }
        }

        return true;
    }

    protected void EnsureUniqueId()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString("N");
        }
    }

    protected virtual int CountItems()
    {
        int count = 0;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty)
            {
                count += slot.Amount;
            }
        }

        return count;
    }

    protected virtual int CountUsedSlots()
    {
        int count = 0;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty)
            {
                count++;
            }
        }

        return count;
    }

    internal virtual void NotifySlotChanged(InventorySlot slot)
    {
        SlotChanged?.Invoke(slot);
        Changed?.Invoke(this);
    }

    protected virtual int CompareItemStacks((Item item, int amount) left, (Item item, int amount) right)
    {
        int result = CompareItems(left.item, right.item, primarySort);

        if (result == 0 && secondarySort != primarySort)
        {
            result = CompareItems(left.item, right.item, secondarySort);
        }

        if (result == 0)
        {
            result = CompareItems(left.item, right.item, InventorySortKey.Name);
        }

        if (result == 0)
        {
            result = StringComparer.OrdinalIgnoreCase.Compare(left.item.Guid, right.item.Guid);
        }

        return result;
    }

    protected virtual int CompareItems(Item left, Item right, InventorySortKey sortKey)
    {
        switch (sortKey)
        {
            case InventorySortKey.Rarity:
                int rarityCompare = GetRaritySortOrder(left.Rarity).CompareTo(GetRaritySortOrder(right.Rarity));
                if (sortRarityDescending)
                {
                    return -rarityCompare;
                }

                return rarityCompare;
            case InventorySortKey.Type:
                return left.Type.CompareTo(right.Type);
            case InventorySortKey.Guid:
                return StringComparer.OrdinalIgnoreCase.Compare(left.Guid, right.Guid);
            case InventorySortKey.Name:
            default:
                return StringComparer.OrdinalIgnoreCase.Compare(left.ItemName, right.ItemName);
        }
    }

    protected virtual int GetRaritySortOrder(string rarityName)
    {
        if (rarityCatalog != null)
        {
            return rarityCatalog.GetSortOrder(rarityName);
        }

        return RarityDefinition.GetFallbackSortOrder(rarityName);
    }

    protected void EnsureInitialized()
    {
        if (!isInitialized)
        {
            InitializeInventory();
        }
    }

    protected void EnsureUIBuild()
    {
        if (!isUIInitialized)
        {
            BuildUI();
        }
    }

    #endregion
}
