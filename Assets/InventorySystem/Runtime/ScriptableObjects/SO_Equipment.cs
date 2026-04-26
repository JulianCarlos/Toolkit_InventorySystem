using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Inventory System/Equipment Data", order = 2)]
public class SO_Equipment : SO_Item
{
    #region Inspector Fields

    [SerializeField, FormerlySerializedAs("EquipmentType")]
    private EquipmentType equipmentType;

    #endregion

    #region Properties

    public EquipmentType EquipmentType => equipmentType;

    #endregion

    #region Configuration

    public void SetEquipmentType(EquipmentType type)
    {
        equipmentType = type;
    }

    #endregion
}
