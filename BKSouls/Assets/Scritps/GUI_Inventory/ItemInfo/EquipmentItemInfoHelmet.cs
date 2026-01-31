using UnityEngine;

namespace BK.Inventory
{
    [CreateAssetMenu(menuName = "Item/Helmet")]
    public class EquipmentItemInfoHelmet : EquipmentItemInfo
    {
        [Header("Helmet")] [HideInInspector] public float extraPhysicalAbsorption = 0;
        [HideInInspector] public float extraMagicalAbsorption = 0;
        [Range(0, 5)] public int extraActionPoint = 1;
    }
}
