using UnityEngine;

public class DemoEquipmentInventory : Inventory
{
    [Header("Equipment Placeholders")]
    [SerializeField] private Texture2D helmetPlaceholder;
    [SerializeField] private Texture2D chestpiecePlaceholder;
    [SerializeField] private Texture2D leggingsPlaceholder;
    [SerializeField] private Texture2D glovesPlaceholder;
    [SerializeField] private Texture2D bootsPlaceholder;
    [SerializeField] private Texture2D beltPlaceholder;
    [SerializeField] private Texture2D ringPlaceholder;
    [SerializeField] private Texture2D amuletPlaceholder;

    protected override void InitializeInventory()
    {
        InventorySlot[] slotsArray = new InventorySlot[containerSize];

        if (containerSize >= 9)
        {
            slotsArray[0] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Helmet));
            slotsArray[0].SetPlaceholder(helmetPlaceholder);

            slotsArray[1] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Chestpiece));
            slotsArray[1].SetPlaceholder(chestpiecePlaceholder);

            slotsArray[2] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Gloves));
            slotsArray[2].SetPlaceholder(glovesPlaceholder);

            slotsArray[3] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Leggings));
            slotsArray[3].SetPlaceholder(leggingsPlaceholder);

            slotsArray[4] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Boots));
            slotsArray[4].SetPlaceholder(bootsPlaceholder);

            slotsArray[5] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Belt));
            slotsArray[5].SetPlaceholder(beltPlaceholder);

            slotsArray[6] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Ring1));
            slotsArray[6].SetPlaceholder(ringPlaceholder);

            slotsArray[7] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Ring2));
            slotsArray[7].SetPlaceholder(ringPlaceholder);

            slotsArray[8] = new InventorySlot(this, ItemFilterFactory.ByEquipmentType(EquipmentType.Amulet));
            slotsArray[8].SetPlaceholder(amuletPlaceholder);
        }
        else
        {
            for (int i = 0; i < containerSize; i++)
            {
                slotsArray[i] = new InventorySlot(this);
            }
        }

        slots = slotsArray;
        isInitialized = true;
    }
}
