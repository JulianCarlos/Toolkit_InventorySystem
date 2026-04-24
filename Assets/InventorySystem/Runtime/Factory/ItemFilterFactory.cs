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

    public static Predicate<Item> ByRarity(RarityType minRarity)
    {
        return item => item != null && item.Rarity >= minRarity;
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
}
