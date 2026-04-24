using UnityEngine;

public static class ItemFactory
{
    public static Item CreateItem(SO_Item itemData)
    {
        return CreateItem(itemData, itemData.DefaultRarity);
    }

    public static Item CreateItem(SO_Item itemData, RarityType rarity)
    {
        string iconPath = itemData.Icon != null ? itemData.Icon.name : "";

        Item item;

        if (itemData is SO_Equipment equipmentData)
        {
            item = new Equipment(
                itemData.Guid,
                itemData.ItemName,
                itemData.Description,
                iconPath,
                itemData.StackSize,
                rarity,
                equipmentData.EquipmentType
            );
        }
        else
        {
            item = new Item(
                itemData.Guid,
                itemData.ItemName,
                itemData.Description,
                iconPath,
                itemData.StackSize,
                rarity,
                itemData.ItemType
            );
        }

        // Inject the direct texture reference so it works without Resources/Images/ folder
        if (itemData.Icon != null)
        {
            item.SetIcon(itemData.Icon);
        }

        return item;
    }

    public static Item CreateItem(string guid, string name, string description, string iconPath, int stackSize, RarityType rarity, ItemType type)
    {
        return new Item(guid, name, description, iconPath, stackSize, rarity, type);
    }
}
