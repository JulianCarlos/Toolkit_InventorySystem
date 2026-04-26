using UnityEngine;
using UnityEngine.UIElements;

public class DemoDropInventory : Inventory
{
    [Header("Drop Settings")]
    [SerializeField] private float lifeTime = 120f;

    private float timer;

    protected override void Awake()
    {
        timer = lifeTime;
    }

    public void InitializeDrop(int dropSlotCount, VisualTreeAsset layout, VisualTreeAsset slotAsset)
    {
        layoutTemplate = layout;
        slotTemplate = slotAsset;
        slotCount = dropSlotCount;

        QueryUIReferences();
        InitializeInventory();
    }

    public override VisualElement GetInventoryUI()
    {
        if (isInitialized && inventoryRoot != null)
        {
            if (!isUIInitialized)
            {
                BuildUI();
            }

            return inventoryRoot;
        }

        QueryUIReferences();
        InitializeInventory();
        BuildUI();

        return inventoryRoot;
    }

    public override int TryAddItem(Item item, int amount)
    {
        return amount;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f || IsCompletelyEmpty())
        {
            if (DemoInventoryManager.Instance != null)
            {
                DemoInventoryManager.Instance.CloseInventories();
            }

            Destroy(gameObject);
            return;
        }

        foreach (var slot in ItemSlots)
        {
            if (!slot.IsEmpty)
            {
                slot.ShowSlot();
            }
            else
            {
                slot.HideSlot();
            }
        }
    }

    public bool IsCompletelyEmpty()
    {
        foreach (InventorySlot slot in ItemSlots)
        {
            if (!slot.IsEmpty)
            {
                return false;
            }
        }

        return true;
    }
}
