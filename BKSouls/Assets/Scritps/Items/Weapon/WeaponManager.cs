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
                    // STR 단일 스케일링
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, str, 0.5f);
                    break;

                case WeaponClass.Spear:
                    // STR + DEX 복합 스케일링
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, str, 0.3f)
                                  + StatScalingBonus(weapon.physicalDamage, dex, 0.3f);
                    break;

                case WeaponClass.Bow:
                    // DEX 단일 스케일링
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, dex, 0.5f);
                    break;

                case WeaponClass.Staff:
                    // INT 스케일링 (근접 타격 시)
                    magicBonus = StatScalingBonus(weapon.magicDamage, intel, 0.5f);
                    break;

                //  Faith 스케일링 (버프 구현 시 추가)

                case WeaponClass.MediumShield:
                case WeaponClass.LightShield:
                    break;  // 방패는 스케일링 없음
            }

            // 요구 스탯 미충족 시 데미지 절반
            float requirementPenalty = MeetsStatRequirements(weapon, str, dex, intel, net.faith.Value) ? 1f : 0.5f;

            meleeDamageCollider.physicalDamage = (weapon.physicalDamage + physicalBonus) * requirementPenalty;
            meleeDamageCollider.magicDamage = (weapon.magicDamage + magicBonus) * requirementPenalty;
            meleeDamageCollider.fireDamage = weapon.fireDamage * requirementPenalty;
            meleeDamageCollider.lightningDamage = weapon.lightningDamage * requirementPenalty;
            meleeDamageCollider.holyDamage = weapon.holyDamage * requirementPenalty;
            meleeDamageCollider.poiseDamage = weapon.poiseDamage;

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
    }
}
