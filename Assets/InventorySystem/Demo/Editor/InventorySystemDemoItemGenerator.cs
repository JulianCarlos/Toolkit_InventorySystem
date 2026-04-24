using UnityEditor;
using UnityEngine;

public static class InventorySystemDemoItemGenerator
{
    private struct ItemDef
    {
        public string Name;
        public string Description;
        public int StackSize;
        public ItemType Type;
        public RarityType Rarity;
    }

    private static readonly ItemDef[] items = new ItemDef[]
    {
        new() { Name = "Iron Ore",       Description = "Raw iron ore.",                   StackSize = 20, Type = ItemType.Resource,   Rarity = RarityType.Common },
        new() { Name = "Gold Ore",       Description = "Shiny gold ore.",                 StackSize = 20, Type = ItemType.Resource,   Rarity = RarityType.Uncommon },
        new() { Name = "Wood",           Description = "A sturdy log.",                   StackSize = 50, Type = ItemType.Resource,   Rarity = RarityType.Common },
        new() { Name = "Stone",          Description = "A heavy stone.",                  StackSize = 30, Type = ItemType.Resource,   Rarity = RarityType.Common },
        new() { Name = "Health Potion",  Description = "Restores a small amount of HP.",  StackSize = 10, Type = ItemType.Consumable, Rarity = RarityType.Common },
        new() { Name = "Mana Potion",    Description = "Restores a small amount of MP.",  StackSize = 10, Type = ItemType.Consumable, Rarity = RarityType.Uncommon },
        new() { Name = "Ruby",           Description = "A precious red gem.",             StackSize = 5,  Type = ItemType.Misc,       Rarity = RarityType.Rare },
        new() { Name = "Emerald",        Description = "A precious green gem.",           StackSize = 5,  Type = ItemType.Misc,       Rarity = RarityType.VeryRare },
        new() { Name = "Scroll of Fire", Description = "Unleashes a fireball.",           StackSize = 3,  Type = ItemType.Consumable, Rarity = RarityType.Rare },
        new() { Name = "Ancient Relic",  Description = "A mysterious artifact.",          StackSize = 1,  Type = ItemType.Quest,      Rarity = RarityType.Legendary },
        new() { Name = "Leather Scraps", Description = "Scraps of leather.",              StackSize = 20, Type = ItemType.Resource,   Rarity = RarityType.Common },
        new() { Name = "Dragon Scale",   Description = "Scale of a fallen dragon.",       StackSize = 5,  Type = ItemType.Resource,   Rarity = RarityType.Unique },
    };

    private struct EquipDef
    {
        public string Name;
        public string Description;
        public EquipmentType Slot;
        public RarityType Rarity;
    }

    private static readonly EquipDef[] equipment = new EquipDef[]
    {
        new() { Name = "Iron Helmet",    Description = "Basic head protection.",    Slot = EquipmentType.Helmet,     Rarity = RarityType.Common },
        new() { Name = "Iron Chestplate",Description = "Sturdy chest armor.",       Slot = EquipmentType.Chestpiece, Rarity = RarityType.Common },
        new() { Name = "Leather Gloves", Description = "Light hand protection.",    Slot = EquipmentType.Gloves,     Rarity = RarityType.Uncommon },
        new() { Name = "Steel Leggings", Description = "Heavy leg armor.",          Slot = EquipmentType.Leggings,   Rarity = RarityType.Rare },
        new() { Name = "Travel Boots",   Description = "Comfortable boots.",        Slot = EquipmentType.Boots,      Rarity = RarityType.Common },
        new() { Name = "Utility Belt",   Description = "Belt with many pouches.",   Slot = EquipmentType.Belt,       Rarity = RarityType.Uncommon },
        new() { Name = "Silver Ring",    Description = "A polished silver ring.",    Slot = EquipmentType.Ring1,      Rarity = RarityType.Rare },
        new() { Name = "Gold Amulet",    Description = "An ornate gold necklace.",  Slot = EquipmentType.Amulet,     Rarity = RarityType.VeryRare },
    };

    [MenuItem("Tools/Inventory System/Generate Demo Items")]
    public static void GenerateDemoItems()
    {
        const string folder = "Assets/InventorySystem/Demo/ItemData";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/InventorySystem/Demo"))
            {
                AssetDatabase.CreateFolder("Assets/InventorySystem", "Demo");
            }

            AssetDatabase.CreateFolder("Assets/InventorySystem/Demo", "ItemData");
        }

        int created = 0;

        foreach (var def in items)
        {
            string path = $"{folder}/{def.Name.Replace(" ", "")}.asset";
            if (AssetDatabase.LoadAssetAtPath<SO_Item>(path) != null)
            {
                continue;
            }

            SO_Item so = ScriptableObject.CreateInstance<SO_Item>();
            so.Guid = System.Guid.NewGuid().ToString("N");
            so.ItemName = def.Name;
            so.Description = def.Description;
            so.StackSize = def.StackSize;
            so.ItemType = def.Type;
            so.DefaultRarity = def.Rarity;

            AssetDatabase.CreateAsset(so, path);
            created++;
        }

        foreach (var def in equipment)
        {
            string path = $"{folder}/{def.Name.Replace(" ", "")}.asset";
            if (AssetDatabase.LoadAssetAtPath<SO_Equipment>(path) != null)
            {
                continue;
            }

            SO_Equipment so = ScriptableObject.CreateInstance<SO_Equipment>();
            so.Guid = System.Guid.NewGuid().ToString("N");
            so.ItemName = def.Name;
            so.Description = def.Description;
            so.StackSize = 1;
            so.ItemType = ItemType.Equipment;
            so.DefaultRarity = def.Rarity;
            so.EquipmentType = def.Slot;

            AssetDatabase.CreateAsset(so, path);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Inventory System] Generated {created} demo item assets in {folder}");
    }
}
