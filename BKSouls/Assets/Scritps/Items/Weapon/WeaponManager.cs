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

            var net = characterWieldingWeapon.characterNetworkManager;
            int str = net.strength.Value + net.strengthModifier.Value;
            int dex = net.dexterity.Value;
            int intel = net.intelligence.Value;
            int faith = net.faith.Value;

            CalculateScaledWeaponDamage(
                weapon,
                str,
                dex,
                intel,
                faith,
                out float physicalDamage,
                out float magicDamage,
                out float fireDamage,
                out float lightningDamage,
                out float holyDamage);

            meleeDamageCollider.physicalDamage = physicalDamage;
            meleeDamageCollider.magicDamage = magicDamage;
            meleeDamageCollider.fireDamage = fireDamage;
            meleeDamageCollider.lightningDamage = lightningDamage;
            meleeDamageCollider.holyDamage = holyDamage;
            meleeDamageCollider.poiseDamage = weapon.poiseDamage;

            meleeDamageCollider.bonusEffectType = weapon.bonusEffectType;
            meleeDamageCollider.bonusEffectAmount = weapon.bonusEffectAmount;

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

        public static void CalculateScaledWeaponDamage(
            WeaponItem weapon,
            int str,
            int dex,
            int intel,
            int faith,
            out float physicalDamage,
            out float magicDamage,
            out float fireDamage,
            out float lightningDamage,
            out float holyDamage)
        {
            float physicalBonus = 0f;
            float magicBonus = 0f;
            float holyBonus = 0f;

            switch (weapon.weaponClass)
            {
                case WeaponClass.StraightSword:
                case WeaponClass.Fist:
                case WeaponClass.MediumShield:
                case WeaponClass.LightShield:
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, str, RequirementToScaleFactor(weapon.strengthREQ));
                    break;
                case WeaponClass.Spear:
                case WeaponClass.Bow:
                    physicalBonus = StatScalingBonus(weapon.physicalDamage, dex, RequirementToScaleFactor(weapon.dexREQ));
                    break;
                case WeaponClass.Staff:
                    magicBonus = StatScalingBonus(weapon.magicDamage, intel, RequirementToScaleFactor(weapon.intREQ));
                    break;
            }

            if (weapon.magicDamage > 0 && weapon.intREQ > 0 && weapon.weaponClass != WeaponClass.Staff)
                magicBonus += StatScalingBonus(weapon.magicDamage, intel, RequirementToScaleFactor(weapon.intREQ));

            if (weapon.holyDamage > 0 && weapon.faithREQ > 0)
                holyBonus = StatScalingBonus(weapon.holyDamage, faith, RequirementToScaleFactor(weapon.faithREQ));

            float magicEffectBonus = weapon.bonusEffectType == WeaponBonusEffectType.Magic
                ? weapon.bonusEffectAmount
                : 0f;

            physicalDamage = weapon.physicalDamage + physicalBonus;
            magicDamage = weapon.magicDamage + magicBonus + magicEffectBonus;
            fireDamage = weapon.fireDamage;
            lightningDamage = weapon.lightningDamage;
            holyDamage = weapon.holyDamage + holyBonus;
        }

        public static void CalculateRangedProjectileDamage(
            WeaponItem weapon,
            RangedProjectileItem projectile,
            int str,
            int dex,
            int intel,
            int faith,
            out float physicalDamage,
            out float magicDamage,
            out float fireDamage,
            out float lightningDamage,
            out float holyDamage)
        {
            physicalDamage = projectile != null ? projectile.physicalDamage : 0f;
            magicDamage = projectile != null ? projectile.magicDamage : 0f;
            fireDamage = projectile != null ? projectile.fireDamage : 0f;
            lightningDamage = projectile != null ? projectile.lightningDamage : 0f;
            holyDamage = projectile != null ? projectile.holyDamage : 0f;

            if (weapon == null)
                return;

            CalculateScaledWeaponDamage(
                weapon,
                str,
                dex,
                intel,
                faith,
                out float weaponPhysicalDamage,
                out float weaponMagicDamage,
                out float weaponFireDamage,
                out float weaponLightningDamage,
                out float weaponHolyDamage);

            physicalDamage += weaponPhysicalDamage;
            magicDamage += weaponMagicDamage;
            fireDamage += weaponFireDamage;
            lightningDamage += weaponLightningDamage;
            holyDamage += weaponHolyDamage;
        }

        public static float StatScalingBonus(float baseDamage, int statValue, float scaleFactor)
        {
            if (baseDamage <= 0 || statValue <= 0 || scaleFactor <= 0) return 0f;

            float normalized = Mathf.Clamp01(statValue / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * scaleFactor;
        }

        public static float RequirementToScaleFactor(int requirement)
        {
            return Mathf.Max(0, requirement) / 20f;
        }
    }
}
