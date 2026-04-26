using System;

[Serializable]
public class Equipment : Item
{
    public EquipmentType EquipmentSlot { get; set; }

    public Equipment(string guid, string itemName, string description, string iconPath, int stackSize, string rarity, EquipmentType equipmentType)
        : base(guid, itemName, description, iconPath, stackSize, rarity, ItemType.Equipment)
    {
        EquipmentSlot = equipmentType;
    }
}
