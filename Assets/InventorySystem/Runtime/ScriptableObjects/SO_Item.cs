using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory System/Item Data", order = 1)]
public class SO_Item : ScriptableObject
{
    public string Guid;
    public string ItemName;
    public string Description;
    public Texture2D Icon;
    public int StackSize = 1;
    public ItemType ItemType;
    public RarityType DefaultRarity = RarityType.Common;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Guid))
        {
            Guid = System.Guid.NewGuid().ToString("N");
        }
    }
}
