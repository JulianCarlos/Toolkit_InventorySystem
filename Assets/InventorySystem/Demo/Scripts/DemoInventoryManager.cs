using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DemoInventoryManager : SlotManagerBase<InventorySlot>
{
    public static DemoInventoryManager Instance { get; private set; }

    [Header("Inventories")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private Inventory equipmentInventory;
    [SerializeField] private Inventory hotbarInventory;

    private Inventory targetInventory;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        DemoUIManager.Instance.SetupPlayerInventories(playerInventory, equipmentInventory, hotbarInventory);
        DemoUIManager.Instance.HideAllInventories();
        DemoUIManager.Instance.ShowHotbarOnly();
    }

    protected override void Update()
    {
        base.Update();
        HandleInventoryToggle();
    }

    private void HandleInventoryToggle()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.iKey.wasPressedThisFrame)
        {
            return;
        }

        if (DemoGameStateManager.Instance.CurrentState == DemoGameState.Default)
        {
            OpenPlayerInventory();
        }
        else if (DemoGameStateManager.Instance.CurrentState == DemoGameState.InventoryOpen)
        {
            CloseInventories();
        }
    }

    protected override string SlotCssClass => "inventory-slot";

    protected override bool HasItem(InventorySlot slot)
    {
        return slot.Item != null;
    }

    protected override void ShowDragIcon(InventorySlot slot)
    {
        InventoryUIManager.Instance.ShowDrag(slot.Item?.Icon, mousePositionPanel);
        InventoryUIManager.Instance.HideTooltip();
    }

    protected override void HideDragIcon()
    {
        InventoryUIManager.Instance.HideDrag();
    }

    protected override void ShowTooltip(InventorySlot slot)
    {
        InventoryUIManager.Instance.TooltipUpdate(slot, mousePositionPanel, Time.deltaTime);
    }

    protected override void HideTooltip()
    {
        if (InventoryUIManager.Instance.IsTooltipVisible)
        {
            InventoryUIManager.Instance.HideTooltip();
        }
    }

    protected override void OnExtraButtons(InventorySlot slot)
    {
        if (slot.Item == null)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleTransfer(slot);
        }

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            slot.Parent.SplitSlotItem(slot);
        }
    }

    protected override void OnDragRelease(InventorySlot from, InventorySlot to, Vector2 panelPosition)
    {
        if (!InventoryUIManager.Instance.IsInAnyInventoryBounds(panelPosition))
        {
            from.UpdateSlot(null, 0);
            return;
        }

        if (to != null)
        {
            Inventory.MoveOrSwapSlots(from, to);
        }
    }

    private void HandleTransfer(InventorySlot slot)
    {
        Inventory transferTarget = GetTransferTarget(slot.Parent);
        if (transferTarget != null)
        {
            Inventory.TransferSlot(slot, transferTarget);
        }
    }

    private Inventory GetTransferTarget(Inventory origin)
    {
        if (origin == playerInventory)
        {
            if (targetInventory != null)
            {
                return targetInventory;
            }

            return equipmentInventory;
        }

        if (origin == targetInventory)
        {
            return playerInventory;
        }

        if (origin == hotbarInventory)
        {
            return playerInventory;
        }

        if (origin == equipmentInventory)
        {
            return playerInventory;
        }

        return playerInventory;
    }

    public void AddItemToPlayerInventory(WorldItem worldItem)
    {
        Item newItem = ItemFactory.CreateItem(worldItem.ItemData);
        int leftover = playerInventory.TryAddItem(newItem, worldItem.Amount);
        worldItem.UpdateItemAmount(leftover);
    }

    public void OpenPlayerInventory()
    {
        DemoGameStateManager.Instance.SetState(DemoGameState.InventoryOpen);
        DemoUIManager.Instance.ShowPlayerInventory();
        targetInventory = equipmentInventory;
    }

    public void OpenTargetInventory(Inventory inventory)
    {
        DemoGameStateManager.Instance.SetState(DemoGameState.InventoryOpen);
        targetInventory = inventory;
        DemoUIManager.Instance.ShowTargetInventory(inventory);
    }

    public void CloseInventories()
    {
        DemoUIManager.Instance.ShowHotbarOnly();
        targetInventory = null;
        ForceEndDrag();
        DemoGameStateManager.Instance.SetState(DemoGameState.Default);
    }
}
