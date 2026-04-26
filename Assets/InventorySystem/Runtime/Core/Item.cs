using System;
using UnityEngine;

[Serializable]
public class Item
{
    public string Guid { get => guid; set => guid = value ?? string.Empty; }
    public string ItemName { get => itemName; set => itemName = value ?? string.Empty; }
    public string Description { get => description; set => description = value ?? string.Empty; }
    public string IconPath { get => iconPath; set => iconPath = value ?? string.Empty; }
    public int StackSize { get => stackSize; set => stackSize = Mathf.Max(1, value); }
    public string Rarity { get => RarityDefinition.NormalizeName(rarityName); set => rarityName = RarityDefinition.NormalizeName(value); }
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
    [SerializeField] private string rarityName;
    [SerializeField] private ItemType itemType;

    [NonSerialized] private Texture2D icon;
    [NonSerialized] private Texture2D rarityBackground;

    public Item(string guid, string itemName, string description, string iconPath, int stackSize, string rarity, ItemType itemType)
    {
        Guid = guid;
        ItemName = itemName;
        Description = description;
        IconPath = iconPath;
        StackSize = stackSize;
        Rarity = rarity;
        Type = itemType;

        LoadResources();
    }

    public virtual void Use()
    {
        Debug.Log($"{itemName}: {description}, rarity: {Rarity}, type: {itemType}");
    }

    public void LoadResources()
    {
        if (icon == null && !string.IsNullOrEmpty(iconPath))
        {
            icon = Resources.Load<Texture2D>($"Images/{iconPath}");
        }

        if (rarityBackground == null)
        {
            rarityBackground = IconLibrary.GetImageByRarity(Rarity);
        }
    }
}
