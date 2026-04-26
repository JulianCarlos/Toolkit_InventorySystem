using System;

public static class ItemFilterFactory
{
    public static Predicate<Item> AllowAny()
    {
        return item => true;
    }

    public static Predicate<Item> ByItemType(ItemType type)
    {
        return item => item != null && item.Type == type;
    }

    public static Predicate<Item> ByEquipmentType(EquipmentType type)
    {
        return item =>
        {
            if (item is Equipment eq)
            {
                return eq.EquipmentSlot == type;
            }

            return false;
        };
    }

    public static Predicate<Item> ByRarity(string minRarity, SO_RarityCatalog rarityCatalog = null)
    {
        int requiredSortOrder = GetRaritySortOrder(minRarity, rarityCatalog);
        return item => item != null && GetRaritySortOrder(item.Rarity, rarityCatalog) >= requiredSortOrder;
    }

    public static Predicate<Item> Combine(params Predicate<Item>[] predicates)
    {
        return item =>
        {
            foreach (var predicate in predicates)
            {
                if (!predicate(item))
                {
                    return false;
                }
            }

            return true;
        };
    }

    private static int GetRaritySortOrder(string rarityName, SO_RarityCatalog rarityCatalog)
    {
        if (rarityCatalog != null)
        {
            return rarityCatalog.GetSortOrder(rarityName);
        }

        return RarityDefinition.GetFallbackSortOrder(rarityName);
    }
}
