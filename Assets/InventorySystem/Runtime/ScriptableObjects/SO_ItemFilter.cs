using UnityEngine;

[CreateAssetMenu(fileName = "NewItemFilter", menuName = "Inventory System/Item Filter", order = 3)]
public class SO_ItemFilter : ScriptableObject
{
    public ItemType[] allowedTypes;

    public bool Filter(Item item)
    {
        if (allowedTypes == null || allowedTypes.Length == 0)
        {
            return true;
        }

        foreach (var allowedType in allowedTypes)
        {
            if (item.Type == allowedType)
            {
                return true;
            }
        }

        return false;
    }
}
