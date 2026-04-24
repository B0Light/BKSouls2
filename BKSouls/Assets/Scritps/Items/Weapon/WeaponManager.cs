using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class WeaponManager : MonoBehaviour
    {
        public MeleeWeaponDamageCollider meleeDamageCollider;

        private void Awake()
        {
            meleeDamageCollider = GetComponentInChildren<MeleeWeaponDamageCollider>();
        }

        public void SetWeaponDamage(CharacterManager characterWieldingWeapon, WeaponItem weapon)
        {
            if (meleeDamageCollider == null)
                return;

            meleeDamageCollider.characterCausingDamage = characterWieldingWeapon;

            // 스탯 스케일링 계산
            var net = characterWieldingWeapon.characterNetworkManager;
            int str = net.strength.Value + net.strengthModifier.Value;
            int dex = net.dexterity.Value;
            int intel = net.intelligence.Value;
            // faith: 버프 무기가 구현되면 추가

            float physicalBonus = 0f;
            float magicBonus = 0f;

            switch (weapon.weaponClass)
            {
                case WeaponClass.StraightSword:
                case WeaponClass.Fist:
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, str, 0.5f);
                    break;
                case WeaponClass.Spear:
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, str, 0.3f)
                                  + StatScalingBonus(weapon.physicalDamage, dex, 0.3f);
                    break;
                case WeaponClass.Bow:
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, dex, 0.5f);
                    break;
                case WeaponClass.Staff:
                    magicBonus = StatScalingBonus(weapon.magicDamage, intel, 0.5f);
                    break;
                case WeaponClass.MediumShield:
                case WeaponClass.LightShield:
                    break;
            }

            bool meetsReq = MeetsStatRequirements(weapon, str, dex, intel, net.faith.Value);
            float requirementPenalty = meetsReq ? 1f : 0.5f;

            // 요구 스탯 초과 수치에 대한 추가 스케일링
            float excessBonus = 0f;
            if (meetsReq)
            {
                switch (weapon.weaponClass)
                {
                    case WeaponClass.StraightSword:
                    case WeaponClass.Fist:
                        excessBonus = ExcessScalingBonus(weapon.physicalDamage, str, weapon.strengthREQ);
                        break;
                    case WeaponClass.Spear:
                        excessBonus = ExcessScalingBonus(weapon.physicalDamage, str, weapon.strengthREQ)
                                    + ExcessScalingBonus(weapon.physicalDamage, dex, weapon.dexREQ);
                        break;
                    case WeaponClass.Bow:
                        excessBonus = ExcessScalingBonus(weapon.physicalDamage, dex, weapon.dexREQ);
                        break;
                    case WeaponClass.Staff:
                        magicBonus += ExcessScalingBonus(weapon.magicDamage, intel, weapon.intREQ);
                        break;
                }
            }

            // 요구 스탯 충족 시 Magic 보너스 이펙트는 magicDamage에 직접 반영
            float magicEffectBonus = (meetsReq && weapon.bonusEffectType == WeaponBonusEffectType.Magic)
                ? weapon.bonusEffectAmount : 0f;

            meleeDamageCollider.physicalDamage = (weapon.physicalDamage + physicalBonus + excessBonus) * requirementPenalty;
            meleeDamageCollider.magicDamage    = (weapon.magicDamage + magicBonus + magicEffectBonus) * requirementPenalty;
            meleeDamageCollider.fireDamage     = weapon.fireDamage * requirementPenalty;
            meleeDamageCollider.lightningDamage = weapon.lightningDamage * requirementPenalty;
            meleeDamageCollider.holyDamage     = weapon.holyDamage * requirementPenalty;
            meleeDamageCollider.poiseDamage    = weapon.poiseDamage;

            // Frost/Bleed 보너스 이펙트 — 요구 스탯 미충족 시 비활성
            meleeDamageCollider.bonusEffectType   = meetsReq ? weapon.bonusEffectType : WeaponBonusEffectType.None;
            meleeDamageCollider.bonusEffectAmount = meetsReq ? weapon.bonusEffectAmount : 0;

            meleeDamageCollider.light_Attack_01_Modifier = weapon.light_Attack_01_Modifier;
            meleeDamageCollider.light_Attack_02_Modifier = weapon.light_Attack_02_Modifier;
            meleeDamageCollider.light_Jump_Attack_01_Modifier = weapon.light_Jumping_Attack_01_Modifier;
            meleeDamageCollider.heavy_Attack_01_Modifier = weapon.heavy_Attack_01_Modifier;
            meleeDamageCollider.heavy_Attack_02_Modifier = weapon.heavy_Attack_02_Modifier;
            meleeDamageCollider.heavy_Jump_Attack_01_Modifier = weapon.heavy_Jumping_Attack_01_Modifier;
            meleeDamageCollider.charge_Attack_01_Modifier = weapon.charge_Attack_01_Modifier;
            meleeDamageCollider.charge_Attack_02_Modifier = weapon.charge_Attack_02_Modifier;
            meleeDamageCollider.running_Attack_01_Modifier = weapon.running_Attack_01_Modifier;
            meleeDamageCollider.rolling_Attack_01_Modifier = weapon.rolling_Attack_01_Modifier;
            meleeDamageCollider.backstep_Attack_01_Modifier = weapon.backstep_Attack_01_Modifier;
            
            meleeDamageCollider.dw_Attack_01_Modifier = weapon.dw_Attack_01_Modifier;
            meleeDamageCollider.dw_Attack_02_Modifier = weapon.dw_Attack_02_Modifier;
            meleeDamageCollider.dw_Jump_Attack_01_Modifier = weapon.dw_Jump_Attack_01_Modifier;
            meleeDamageCollider.dw_Run_Attack_01_Modifier = weapon.dw_Run_Attack_01_Modifier;
            meleeDamageCollider.dw_Roll_Attack_01_Modifier = weapon.dw_Roll_Attack_01_Modifier;
            meleeDamageCollider.dw_Backstep_Attack_01_Modifier = weapon.dw_Backstep_Attack_01_Modifier;

        }

        // 요구 스탯 충족 여부 (하나라도 부족하면 false)
        private bool MeetsStatRequirements(WeaponItem weapon, int str, int dex, int intel, int faith)
        {
            if (str   < weapon.strengthREQ) return false;
            if (dex   < weapon.dexREQ)      return false;
            if (intel < weapon.intREQ)      return false;
            if (faith < weapon.faithREQ)    return false;
            return true;
        }

        // 스탯 스케일링 보너스 계산
        // curve: stat=20 → ~40%, stat=40 → ~67%, stat=60 → ~84%, stat=80 → ~95%, stat=99 → ~100% of scaleFactor
        private float StatScalingBonus(float baseDamage, int statValue, float scaleFactor)
        {
            float normalized = Mathf.Clamp01(statValue / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * scaleFactor;
        }

        // 요구 스탯 초과분에 대한 추가 보너스 (메인 스케일링보다 작은 계수 0.15f)
        private float ExcessScalingBonus(float baseDamage, int statValue, int requirement)
        {
            int excess = Mathf.Max(0, statValue - requirement);
            float normalized = Mathf.Clamp01(excess / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * 0.15f;
        }
    }
}
