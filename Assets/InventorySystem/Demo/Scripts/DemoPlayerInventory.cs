using UnityEngine;
using UnityEngine.UIElements;

public class DemoPlayerInventory : Inventory
{
    [Header("Player Inventory")]
    [SerializeField] private Button dropAllButton;

    protected override void QueryUIReferences()
    {
        base.QueryUIReferences();
        dropAllButton = inventoryRoot.Q<Button>("DropAllButton");
    }

    protected override void LinkCallbacks()
    {
        base.LinkCallbacks();

        if (dropAllButton != null)
        {
            dropAllButton.clicked += DropAll;
        }
    }

    public void DropAll()
    {
        Debug.Log("[DemoPlayerInventory] Dropping all items!");
        ClearAllSlots();
    }
}
