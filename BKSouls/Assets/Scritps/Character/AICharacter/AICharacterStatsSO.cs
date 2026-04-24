using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "DATA/AI Stats/AI Character Stats")]
    public class AICharacterStatsSO : ScriptableObject
    {
        [Header("Base Resources")]
        public int maxHealth = 400;
        public int maxStamina = 150;

        [Header("Runes")]
        public int runesDroppedOnDeath = 50;

        [Header("Armor Absorption")]
        public float armorPhysicalDamageAbsorption;
        public float armorMagicDamageAbsorption;
        public float armorFireDamageAbsorption;
        public float armorHolyDamageAbsorption;
        public float armorLightningDamageAbsorption;

        [Header("Armor Resistances")]
        public float armorImmunity;
        public float armorRobustness;
        public float armorFocus;
        public float armorVitality;

        [Header("Poise")]
        public float basePoiseDefense;
    }
}
