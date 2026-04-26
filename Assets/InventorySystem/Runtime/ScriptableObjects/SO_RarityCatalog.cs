using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRarityCatalog", menuName = "Inventory System/Rarity Catalog", order = 0)]
public class SO_RarityCatalog : ScriptableObject
{
    #region Inspector Fields

    [SerializeField] private List<RarityDefinition> rarities = CreateDefaultRarities();

    #endregion

    #region Properties

    public IReadOnlyList<RarityDefinition> Rarities => rarities;

    #endregion

    #region Lookup

    public bool TryGetDefinition(string rarityName, out RarityDefinition definition)
    {
        if (rarities == null)
        {
            definition = null;
            return false;
        }

        string normalizedName = RarityDefinition.NormalizeName(rarityName);

        for (int i = 0; i < rarities.Count; i++)
        {
            if (rarities[i] != null && rarities[i].Matches(normalizedName))
            {
                definition = rarities[i];
                return true;
            }
        }

        definition = null;
        return false;
    }

    public int GetSortOrder(string rarityName)
    {
        if (TryGetDefinition(rarityName, out RarityDefinition definition))
        {
            return definition.SortOrder;
        }

        return int.MinValue;
    }

    public Color GetColor(string rarityName)
    {
        if (TryGetDefinition(rarityName, out RarityDefinition definition))
        {
            return definition.Color;
        }

        return RarityDefinition.GetFallbackColor(rarityName);
    }

    public Texture2D GetBackground(string rarityName)
    {
        if (TryGetDefinition(rarityName, out RarityDefinition definition))
        {
            return definition.Background;
        }

        return null;
    }

    public string[] GetRarityNames()
    {
        if (rarities == null)
        {
            return Array.Empty<string>();
        }

        string[] names = new string[rarities.Count];

        for (int i = 0; i < rarities.Count; i++)
        {
            if (rarities[i] != null)
            {
                names[i] = rarities[i].Name;
            }
            else
            {
                names[i] = RarityDefinition.DefaultName;
            }
        }

        return names;
    }

    #endregion

    #region Defaults

    [ContextMenu("Reset To Default Rarities")]
    public void ResetToDefaults()
    {
        rarities = CreateDefaultRarities();
    }

    private void Reset()
    {
        ResetToDefaults();
    }

    #endregion

    #region Validation

    private void OnValidate()
    {
        if (rarities == null)
        {
            rarities = new List<RarityDefinition>();
            return;
        }

        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < rarities.Count; i++)
        {
            if (rarities[i] == null)
            {
                rarities[i] = new RarityDefinition($"Rarity {i + 1}", i, Color.white);
            }

            string rarityName = RarityDefinition.NormalizeName(rarities[i].Name);
            if (!knownNames.Add(rarityName))
            {
                rarityName = $"{rarityName} {i + 1}";
            }

            rarities[i].Name = rarityName;
        }
    }

    #endregion

    #region Factory

    private static List<RarityDefinition> CreateDefaultRarities()
    {
        return new List<RarityDefinition>
        {
            new RarityDefinition("Common", 0, new Color(0.66f, 0.66f, 0.66f)),
            new RarityDefinition("Uncommon", 10, new Color(0.18f, 0.80f, 0.25f)),
            new RarityDefinition("Rare", 20, new Color(0.24f, 0.55f, 0.95f)),
            new RarityDefinition("Very Rare", 30, new Color(0.70f, 0.30f, 0.90f)),
            new RarityDefinition("Legendary", 40, new Color(1.00f, 0.60f, 0.10f)),
            new RarityDefinition("Unique", 50, new Color(0.95f, 0.85f, 0.15f))
        };
    }

    #endregion
}