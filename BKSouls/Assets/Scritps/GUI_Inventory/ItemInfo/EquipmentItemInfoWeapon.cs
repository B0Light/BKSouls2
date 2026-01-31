using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * physical Damage : ENEMY
 *
 * Slash / Stab : player Attack
 */
namespace BK.Inventory
{
    public class EquipmentItemInfoWeapon : EquipmentItemInfo
    {
        [Header("Animations")] public AnimatorOverrideController weaponAnimator;

        [Header("Weapon Equip Sprite")] public Sprite weaponEquipSprite;

        [Header("Weapon Base Damage")] public int physicalDamage = 0;
        public int magicalDamage = 0;

        public float attackSpeed = 1.0f;
        public float hAtkMod01 = 1.0f;
        public float hAtkMod02 = 1.2f;
        public float hAtkMod03 = 2.0f;
        public float vAtkMod01 = 1.4f;
        public float vAtkMod02 = 1.6f;
        public float vAtkMod03 = 2.0f;
        public float runningAtkMod = 1.1f;
        public float rollingAtkMod = 2.0f;
        public float backStepAtkMod = 1.5f;
        public float jumpingAtkMod = 5.0f;

        [Header("Stamina Costs Modifiers")] public int baseActionCost = 1;

        [Header("Weapon Blocking Absorption")] public int physicalDamageAbsorption = 50;
        public int magicalDamageAbsorption = 50;
        public int stability = 50; // 가드시 poiseDmg 가드 정도 
    }
}