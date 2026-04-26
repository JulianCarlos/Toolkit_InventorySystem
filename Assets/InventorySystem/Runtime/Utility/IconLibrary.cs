using UnityEngine;

public static class IconLibrary
{
    private const string ImagesFolder = "Images/";
    private const string RarityFolder = "Rarity/";

    public static Texture2D GetImageByRarity(string rarityName)
    {
        string resourceName = RarityDefinition.ToResourceName(rarityName);
        string path = string.Concat(ImagesFolder, RarityFolder, resourceName);
        return Resources.Load<Texture2D>(path);
    }
}
