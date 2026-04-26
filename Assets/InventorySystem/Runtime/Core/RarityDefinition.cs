using System;
using System.Text;
using UnityEngine;

[Serializable]
public class RarityDefinition
{
    public const string DefaultName = "Common";

    public string Name
    {
        get => NormalizeName(name);
        set => name = NormalizeName(value);
    }

    public int SortOrder
    {
        get => sortOrder;
        set => sortOrder = value;
    }

    public Color Color
    {
        get => color;
        set => color = value;
    }

    public Texture2D Background
    {
        get => background;
        set => background = value;
    }

    [SerializeField, Tooltip("Displayed and stored rarity name. Item assets reference this value as a string.")]
    private string name = DefaultName;

    [SerializeField, Tooltip("Higher values sort as rarer when rarity sorting is descending.")]
    private int sortOrder;

    [SerializeField, Tooltip("Inspector accent color for this rarity.")]
    private Color color = Color.white;

    [SerializeField, Tooltip("Optional slot background used for items with this rarity.")]
    private Texture2D background;

    public RarityDefinition()
    {
    }

    public RarityDefinition(string name, int sortOrder, Color color, Texture2D background = null)
    {
        this.name = NormalizeName(name);
        this.sortOrder = sortOrder;
        this.color = color;
        this.background = background;
    }

    public bool Matches(string rarityName)
    {
        return string.Equals(Name, NormalizeName(rarityName), StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeName(string rarityName)
    {
        return string.IsNullOrWhiteSpace(rarityName)
            ? DefaultName
            : rarityName.Trim();
    }

    public static string ToResourceName(string rarityName)
    {
        string normalized = NormalizeName(rarityName);
        var builder = new StringBuilder(normalized.Length);

        for (int i = 0; i < normalized.Length; i++)
        {
            char character = normalized[i];
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        if (builder.Length > 0)
        {
            return builder.ToString();
        }

        return DefaultName.ToLowerInvariant();
    }

    public static Color GetFallbackColor(string rarityName)
    {
        switch (ToResourceName(rarityName))
        {
            case "common":
                return new Color(0.66f, 0.66f, 0.66f);
            case "uncommon":
                return new Color(0.18f, 0.80f, 0.25f);
            case "rare":
                return new Color(0.24f, 0.55f, 0.95f);
            case "veryrare":
                return new Color(0.70f, 0.30f, 0.90f);
            case "legendary":
                return new Color(1.00f, 0.60f, 0.10f);
            case "unique":
                return new Color(0.95f, 0.85f, 0.15f);
            default:
                return Color.white;
        }
    }

    public static int GetFallbackSortOrder(string rarityName)
    {
        switch (ToResourceName(rarityName))
        {
            case "unique":
                return 50;
            case "legendary":
                return 40;
            case "veryrare":
                return 30;
            case "rare":
                return 20;
            case "uncommon":
                return 10;
            case "common":
                return 0;
            default:
                return int.MinValue;
        }
    }
}