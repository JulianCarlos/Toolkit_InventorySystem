using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class Inventory : SlotContainer<Item>
{
    public string UniqueId => uniqueId;
    public string InventoryName => inventoryName;
    public InventorySlot[] ItemSlots => (InventorySlot[])slots;
    public VisualElement InventoryRoot => inventoryRoot;
    public bool IsInitialized => isInitialized;

    [Header("Inventory Settings")]
    [SerializeField] protected string uniqueId;
    [SerializeField] protected string inventoryName = "Inventory";
    [SerializeField] protected VisualTreeAsset layoutTemplate;
    [SerializeField] protected VisualTreeAsset slotTemplate;
    [SerializeField] protected int slotCount;

    [Header("Filters")]
    [SerializeField] protected SO_ItemFilter[] filters = new SO_ItemFilter[0];

    protected VisualElement inventoryRoot;
    protected VisualElement slotContainer;
    protected Button sortButton;

    protected bool isInitialized;
    protected bool isUIInitialized;

    protected virtual void Awake()
    {
        if (layoutTemplate == null)
        {
            // Headless mode: logic-only inventory with no UI (useful for tests)
            if (slotCount > 0)
            {
                containerSize = slotCount;
            }
            else if (containerSize <= 0)
            {
                containerSize = 20;
            }

            InitializeInventory();
            return;
        }

        QueryUIReferences();
        InitializeInventory();
    }

    protected void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
        }
    }

    #region Item Operations

    public virtual int TryAddItem(Item item, int amount)
    {
        if (item == null || amount <= 0) return amount;

        if (!PassesInventoryFilters(item))
        {
            return amount;
        }

        int remaining = amount;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (remaining <= 0) break;
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
            if (remaining <= 0) break;
            if (slot.IsEmpty && slot.IsItemAllowed(item))
            {
                int toAdd = Mathf.Min(item.StackSize, remaining);
                slot.UpdateSlot(item, toAdd);
                remaining -= toAdd;
            }
        }

        return remaining;
    }

    public int RemoveItem(string itemGuid, int amount)
    {
        if (string.IsNullOrEmpty(itemGuid) || amount <= 0) return amount;

        int remaining = amount;

        foreach (InventorySlot slot in ItemSlots)
        {
            if (remaining <= 0) break;
            if (!slot.IsEmpty && slot.Item?.Guid == itemGuid)
            {
                int remove = Mathf.Min(slot.Amount, remaining);
                slot.UpdateSlot(slot.Item, slot.Amount - remove);
                remaining -= remove;
            }
        }

        return remaining;
    }

    public int GetItemCount(string itemGuid)
    {
        int count = 0;
        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty && slot.Item != null && slot.Item.Guid == itemGuid)
                count += slot.Amount;
        }
        return count;
    }

    public bool HasItem(string itemGuid, int amount)
    {
        return GetItemCount(itemGuid) >= amount;
    }

    public void SplitSlotItem(InventorySlot slot)
    {
        if (slot.Amount <= 1)
        {
            return;
        }

        InventorySlot freeSlot = GetFreeInventorySlot();

        if (freeSlot != null)
        {
            int half = slot.Amount / 2;
            slot.UpdateSlot(slot.Item, slot.Amount - half);
            freeSlot.UpdateSlot(slot.Item, half);
        }
    }

    public static void TransferItem(InventorySlot sourceSlot, Inventory targetInventory)
    {
        if (sourceSlot == null || sourceSlot.IsEmpty || targetInventory == null)
        {
            return;
        }

        int remaining = targetInventory.TryAddItem(sourceSlot.Item, sourceSlot.Amount);
        sourceSlot.UpdateSlot(remaining);
    }

    #endregion

    #region Sort

    public void CompactAndSort()
    {
        var itemDict = new Dictionary<(string guid, RarityType rarity), (Item item, int amount)>();

        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty && slot.Item != null)
            {
                var key = (slot.Item.Guid, slot.Item.Rarity);
                if (itemDict.TryGetValue(key, out var entry))
                {
                    itemDict[key] = (slot.Item, entry.amount + slot.Amount);
                }
                else
                {
                    itemDict[key] = (slot.Item, slot.Amount);
                }
            }
        }

        List<(Item item, int amount)> sortedItems = itemDict.Values
            .OrderBy(x => x.item.ItemName, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(x => x.item.Rarity)
            .ThenBy(x => x.item.Guid)
            .ToList();

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

    #endregion

    #region Serialization

    public virtual void SetInventoryData(InventoryData data)
    {
        if (data == null) return;

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
        VisualElement[] children = slotContainer.Children().ToArray();

        for (int i = 0; i < children.Length && i < ItemSlots.Length; i++)
        {
            ItemSlots[i].InitializeUI(children[i]);
            children[i].userData = ItemSlots[i];
            children[i].AddToClassList("inventory-slot");
        }

        LinkCallbacks();

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RegisterInventoryUI(inventoryRoot);
        }

        isUIInitialized = true;
    }

    protected virtual void OnDestroy()
    {
        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.UnregisterInventoryUI(inventoryRoot);
        }
    }

    protected virtual void LinkCallbacks()
    {
        if (sortButton != null)
        {
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
        foreach (InventorySlot slot in ItemSlots)
        {
            if (slot.IsEmpty) return slot;
        }
        return null;
    }

    protected bool PassesInventoryFilters(Item item)
    {
        if (item == null || filters.Length == 0) return true;

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i] != null && !filters[i].Filter(item)) return false;
        }
        return true;
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
