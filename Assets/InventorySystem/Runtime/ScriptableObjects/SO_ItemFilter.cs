using System.Collections.Generic;
using UnityEngine;

public interface IItemFilter
{
    bool Allows(Item item);
}

[CreateAssetMenu(fileName = "NewItemFilter", menuName = "Inventory System/Item Filter", order = 3)]
public class SO_ItemFilter : ScriptableObject, IItemFilter
{
    #region Inspector Fields

    [SerializeField] private ItemType[] allowedTypes = new ItemType[0];

    #endregion

    #region Properties

    public IReadOnlyList<ItemType> AllowedTypes => allowedTypes;

    #endregion

    #region Filtering

    public bool Allows(Item item)
    {
        return Filter(item);
    }

    public bool Filter(Item item)
    {
        if (item == null)
        {
            return false;
        }

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

    #endregion
}
