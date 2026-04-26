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
        public string Rarity;
    }

    private static readonly ItemDef[] items = new ItemDef[]
    {
        new() { Name = "Iron Ore",       Description = "Raw iron ore.",                   StackSize = 20, Type = ItemType.Resource,   Rarity = "Common" },
        new() { Name = "Gold Ore",       Description = "Shiny gold ore.",                 StackSize = 20, Type = ItemType.Resource,   Rarity = "Uncommon" },
        new() { Name = "Wood",           Description = "A sturdy log.",                   StackSize = 50, Type = ItemType.Resource,   Rarity = "Common" },
        new() { Name = "Stone",          Description = "A heavy stone.",                  StackSize = 30, Type = ItemType.Resource,   Rarity = "Common" },
        new() { Name = "Health Potion",  Description = "Restores a small amount of HP.",  StackSize = 10, Type = ItemType.Consumable, Rarity = "Common" },
        new() { Name = "Mana Potion",    Description = "Restores a small amount of MP.",  StackSize = 10, Type = ItemType.Consumable, Rarity = "Uncommon" },
        new() { Name = "Ruby",           Description = "A precious red gem.",             StackSize = 5,  Type = ItemType.Misc,       Rarity = "Rare" },
        new() { Name = "Emerald",        Description = "A precious green gem.",           StackSize = 5,  Type = ItemType.Misc,       Rarity = "Very Rare" },
        new() { Name = "Scroll of Fire", Description = "Unleashes a fireball.",           StackSize = 3,  Type = ItemType.Consumable, Rarity = "Rare" },
        new() { Name = "Ancient Relic",  Description = "A mysterious artifact.",          StackSize = 1,  Type = ItemType.Quest,      Rarity = "Legendary" },
        new() { Name = "Leather Scraps", Description = "Scraps of leather.",              StackSize = 20, Type = ItemType.Resource,   Rarity = "Common" },
        new() { Name = "Dragon Scale",   Description = "Scale of a fallen dragon.",       StackSize = 5,  Type = ItemType.Resource,   Rarity = "Unique" },
    };

    private struct EquipDef
    {
        public string Name;
        public string Description;
        public EquipmentType Slot;
        public string Rarity;
    }

    private static readonly EquipDef[] equipment = new EquipDef[]
    {
        new() { Name = "Iron Helmet",     Description = "Basic head protection.",   Slot = EquipmentType.Helmet,     Rarity = "Common" },
        new() { Name = "Iron Chestplate", Description = "Sturdy chest armor.",      Slot = EquipmentType.Chestpiece, Rarity = "Common" },
        new() { Name = "Leather Gloves",  Description = "Light hand protection.",   Slot = EquipmentType.Gloves,     Rarity = "Uncommon" },
        new() { Name = "Steel Leggings",  Description = "Heavy leg armor.",         Slot = EquipmentType.Leggings,   Rarity = "Rare" },
        new() { Name = "Travel Boots",    Description = "Comfortable boots.",       Slot = EquipmentType.Boots,      Rarity = "Common" },
        new() { Name = "Utility Belt",    Description = "Belt with many pouches.",  Slot = EquipmentType.Belt,       Rarity = "Uncommon" },
        new() { Name = "Silver Ring",     Description = "A polished silver ring.",  Slot = EquipmentType.Ring1,      Rarity = "Rare" },
        new() { Name = "Gold Amulet",     Description = "An ornate gold necklace.", Slot = EquipmentType.Amulet,     Rarity = "Very Rare" },
    };

    [MenuItem("Tools/Inventory System/Generate Demo Items")]
    public static void GenerateDemoItems()
    {
        const string folder = "Assets/InventorySystem/Demo/ItemData";
        const string rarityCatalogPath = "Assets/InventorySystem/Runtime/ScriptableObjects/DefaultRarityCatalog.asset";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/InventorySystem/Demo"))
            {
                AssetDatabase.CreateFolder("Assets/InventorySystem", "Demo");
            }

            AssetDatabase.CreateFolder("Assets/InventorySystem/Demo", "ItemData");
        }

        int created = 0;
    SO_RarityCatalog rarityCatalog = AssetDatabase.LoadAssetAtPath<SO_RarityCatalog>(rarityCatalogPath);

        foreach (var def in items)
        {
            string path = $"{folder}/{def.Name.Replace(" ", "")}.asset";
            if (AssetDatabase.LoadAssetAtPath<SO_Item>(path) != null)
            {
                continue;
            }

            SO_Item so = ScriptableObject.CreateInstance<SO_Item>();
            so.Configure(System.Guid.NewGuid().ToString("N"), def.Name, def.Description, null, def.StackSize, def.Type, def.Rarity, rarityCatalog);

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
            so.Configure(System.Guid.NewGuid().ToString("N"), def.Name, def.Description, null, 1, ItemType.Equipment, def.Rarity, rarityCatalog);
            so.SetEquipmentType(def.Slot);

            AssetDatabase.CreateAsset(so, path);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Inventory System] Generated {created} demo item assets in {folder}");
    }
}
