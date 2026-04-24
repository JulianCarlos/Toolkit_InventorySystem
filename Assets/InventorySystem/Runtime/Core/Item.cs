using System;
using UnityEngine;

[Serializable]
public class Item
{
    public string Guid { get => guid; set => guid = value; }
    public string ItemName { get => itemName; set => itemName = value; }
    public string Description { get => description; set => description = value; }
    public string IconPath { get => iconPath; set => iconPath = value; }
    public int StackSize { get => stackSize; set => stackSize = value; }
    public RarityType Rarity { get => rarity; set => rarity = value; }
    public ItemType Type { get => itemType; set => itemType = value; }

    public Texture2D Icon => icon;
    public Texture2D RarityBackground => rarityBackground;

    public void SetIcon(Texture2D texture) => icon = texture;
    public void SetRarityBackground(Texture2D texture) => rarityBackground = texture;

    [SerializeField] private string guid;
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private string iconPath;
    [SerializeField] private int stackSize;
    [SerializeField] private RarityType rarity;
    [SerializeField] private ItemType itemType;

    [NonSerialized] private Texture2D icon;
    [NonSerialized] private Texture2D rarityBackground;

    public Item(string guid, string itemName, string description, string iconPath, int stackSize, RarityType rarity, ItemType itemType)
    {
        this.guid = guid;
        this.itemName = itemName;
        this.description = description;
        this.iconPath = iconPath;
        this.stackSize = Mathf.Max(1, stackSize);
        this.rarity = rarity;
        this.itemType = itemType;

        LoadResources();
    }

    public virtual void Use()
    {
        Debug.Log($"{itemName}: {description}, rarity: {rarity}, type: {itemType}");
    }

    public void LoadResources()
    {
        if (icon == null && !string.IsNullOrEmpty(iconPath))
        {
            icon = Resources.Load<Texture2D>($"Images/{iconPath}");
        }

        if (rarityBackground == null)
        {
            rarityBackground = IconLibrary.GetImageByRarity(rarity);
        }
    }
}
