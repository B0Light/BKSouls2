using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Items/Weapons/Melee Weapon")]
    public class MeleeWeaponItem : WeaponItem
    {
        [Header("Attack Modifiers")]
        public float riposte_Attack_01_Modifier = 3.3f;
        public float backstab_Attack_01_Modifier = 3.3f;
        //  WEAPON "DEFLECTION" (If the weapon will bounce off another weapon when it is being guarded against)
        //  CAN BE BUFFED
    }
}
