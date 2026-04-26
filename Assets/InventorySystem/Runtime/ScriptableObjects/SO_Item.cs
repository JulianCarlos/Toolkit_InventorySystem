using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory System/Item Data", order = 1)]
public class SO_Item : ScriptableObject
{
    #region Inspector Fields

    [SerializeField, FormerlySerializedAs("Guid")] private string guid;
    [SerializeField, FormerlySerializedAs("ItemName")] private string itemName;
    [SerializeField, FormerlySerializedAs("Description"), TextArea] private string description;
    [SerializeField, FormerlySerializedAs("Icon")] private Texture2D icon;
    [SerializeField, FormerlySerializedAs("StackSize"), Min(1)] private int stackSize = 1;
    [SerializeField, FormerlySerializedAs("ItemType")] private ItemType itemType;
    [SerializeField] private SO_RarityCatalog rarityCatalog;
    [SerializeField] private string defaultRarityName;

    #endregion

    #region Properties

    public string Guid => guid;
    public string ItemName => itemName;
    public string Description => description;
    public Texture2D Icon => icon;
    public int StackSize => stackSize;
    public ItemType ItemType => itemType;

    public SO_RarityCatalog RarityCatalog
    {
        get => rarityCatalog;
        set => rarityCatalog = value;
    }

    public string DefaultRarity
    {
        get => RarityDefinition.NormalizeName(defaultRarityName);
        set => defaultRarityName = RarityDefinition.NormalizeName(value);
    }

    #endregion

    #region Configuration

    public void Configure(string guid, string itemName, string description, Texture2D icon, int stackSize, ItemType itemType, string defaultRarity, SO_RarityCatalog rarityCatalog = null)
    {
        if (string.IsNullOrWhiteSpace(guid))
        {
            this.guid = System.Guid.NewGuid().ToString("N");
        }
        else
        {
            this.guid = guid;
        }

        this.itemName = itemName ?? string.Empty;
        this.description = description ?? string.Empty;
        this.icon = icon;
        this.stackSize = Mathf.Max(1, stackSize);
        this.itemType = itemType;
        this.rarityCatalog = rarityCatalog;
        DefaultRarity = defaultRarity;
    }

    #endregion

    #region Rarity

    public Texture2D GetRarityBackground(string rarityName = null)
    {
        string resolvedRarity = rarityName;

        if (string.IsNullOrWhiteSpace(resolvedRarity))
        {
            resolvedRarity = DefaultRarity;
        }

        if (rarityCatalog != null)
        {
            Texture2D catalogBackground = rarityCatalog.GetBackground(resolvedRarity);
            if (catalogBackground != null)
            {
                return catalogBackground;
            }
        }

        return IconLibrary.GetImageByRarity(resolvedRarity);
    }

    #endregion

    #region Unity Lifecycle

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(guid))
        {
            guid = System.Guid.NewGuid().ToString("N");
        }

        stackSize = Mathf.Max(1, stackSize);
        defaultRarityName = RarityDefinition.NormalizeName(defaultRarityName);
    }

    #endregion
}
