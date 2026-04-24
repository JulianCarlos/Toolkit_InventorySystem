using UnityEngine;

[System.Serializable]
public class InventoryData
{
    public string InventoryGuid;
    public string InventoryName;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
    public string PrefabName;
    public InventorySlotData[] InventorySlots;
}

[System.Serializable]
public class InventorySlotData
{
    public Item InventoryItem;
    public int Amount;
}
