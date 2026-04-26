using UnityEngine;

public static class ItemFactory
{
    /// <summary>
    /// Creates a runtime item using the default rarity configured on the item asset.
    /// </summary>
    public static Item CreateItem(SO_Item itemData)
    {
        if (itemData == null)
        {
            return null;
        }

        return CreateItem(itemData, itemData.DefaultRarity);
    }

    /// <summary>
    /// Creates a runtime item with a caller-supplied rarity name.
    /// </summary>
    public static Item CreateItem(SO_Item itemData, string rarity)
    {
        if (itemData == null)
        {
            return null;
        }

        string iconPath = string.Empty;

        if (itemData.Icon != null)
        {
            iconPath = itemData.Icon.name;
        }

        string rarityName = RarityDefinition.NormalizeName(rarity);

        Item item;

        if (itemData is SO_Equipment equipmentData)
        {
            item = new Equipment(
                itemData.Guid,
                itemData.ItemName,
                itemData.Description,
                iconPath,
                itemData.StackSize,
                rarityName,
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
                rarityName,
                itemData.ItemType
            );
        }

        if (itemData.Icon != null)
        {
            item.SetIcon(itemData.Icon);
        }

        item.SetRarityBackground(itemData.GetRarityBackground(rarityName));

        return item;
    }

    public static Item CreateItem(string guid, string name, string description, string iconPath, int stackSize, string rarity, ItemType type)
    {
        return new Item(guid, name, description, iconPath, stackSize, rarity, type);
    }
}
