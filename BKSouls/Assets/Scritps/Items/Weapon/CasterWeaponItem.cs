using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Items/Weapons/Caster Weapon")]
    public class CasterWeaponItem : WeaponItem
    {
        [Header("Spell Power")]
        [Tooltip("무기의 기본 마법 위력. 100 = 기준값. 높을수록 주문 데미지 증가")]
        public float spellBuff = 100f;
    }
}
