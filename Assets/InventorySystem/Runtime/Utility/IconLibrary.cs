using UnityEngine;

public static class IconLibrary
{
    private const string ImagesFolder = "Images/";
    private const string RarityFolder = "Rarity/";

    public static Texture2D GetImageByRarity(RarityType rarity)
    {
        string rarityName = rarity.ToString().ToLower();
        string path = string.Concat(ImagesFolder, RarityFolder, rarityName);
        return Resources.Load<Texture2D>(path);
    }
}
