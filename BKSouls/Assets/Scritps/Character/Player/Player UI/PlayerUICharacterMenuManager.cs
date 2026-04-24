using TMPro;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// 플레이어의 기본 스탯과 장착 장비를 기반으로 실제 능력치를 표시하는 UI
    /// </summary>
    public class PlayerUICharacterMenuManager : PlayerUIMenu
    {
        private PlayerManager Player => GUIController.Instance.localPlayer;



        // ── 파생 자원 ──────────────────────────────────────────────────
        [Header("Derived Resources")]
        [SerializeField] private TextMeshProUGUI maxHPText;
        [SerializeField] private TextMeshProUGUI maxFPText;
        [SerializeField] private TextMeshProUGUI maxStaminaText;

        // ── 오른손 무기 공격력 ─────────────────────────────────────────
        [Header("Right Hand Attack")]
        [SerializeField] private TextMeshProUGUI rightPhysicalAtkText;
        [SerializeField] private TextMeshProUGUI rightMagicAtkText;
        [SerializeField] private TextMeshProUGUI rightFireAtkText;
        [SerializeField] private TextMeshProUGUI rightLightningAtkText;
        [SerializeField] private TextMeshProUGUI rightHolyAtkText;

        // ── 왼손 무기 공격력 ───────────────────────────────────────────
        [Header("Left Hand Attack")]
        [SerializeField] private TextMeshProUGUI leftPhysicalAtkText;
        [SerializeField] private TextMeshProUGUI leftMagicAtkText;
        [SerializeField] private TextMeshProUGUI leftFireAtkText;
        [SerializeField] private TextMeshProUGUI leftLightningAtkText;
        [SerializeField] private TextMeshProUGUI leftHolyAtkText;

        // ── 방어력 ─────────────────────────────────────────────────────
        [Header("Defense")]
        [SerializeField] private TextMeshProUGUI physicalDefText;
        [SerializeField] private TextMeshProUGUI magicDefText;
        [SerializeField] private TextMeshProUGUI fireDefText;
        [SerializeField] private TextMeshProUGUI lightningDefText;
        [SerializeField] private TextMeshProUGUI holyDefText;
        [SerializeField] private TextMeshProUGUI poiseText;

        // ── 기본 스탯 ──────────────────────────────────────────────────
        [Header("Attributes")]
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI faithText;

        // ── 상태 저항 ──────────────────────────────────────────────────
        [Header("Status Resistance")]
        [SerializeField] private TextMeshProUGUI immunityText;
        [SerializeField] private TextMeshProUGUI robustnessText;
        [SerializeField] private TextMeshProUGUI focusResistText;
        [SerializeField] private TextMeshProUGUI vitalityText;

        // ──────────────────────────────────────────────────────────────

        public override void OpenMenu()
        {
            base.OpenMenu();
            Refresh();
        }

        /// <summary>장비 변경 등 외부에서 수동 갱신이 필요할 때 호출</summary>
        public void Refresh()
        {
            if (Player == null) return;

            RefreshAttributes();
            RefreshResources();
            RefreshAttack();
            RefreshDefense();
        }

        // ── 기본 스탯 ──────────────────────────────────────────────────

        private void RefreshAttributes()
        {
            var net = Player.playerNetworkManager;
            if (strengthText != null)    strengthText.SetText((net.strength.Value + net.strengthModifier.Value).ToString());
            if (dexterityText != null)   dexterityText.SetText(net.dexterity.Value.ToString());
            if (intelligenceText != null) intelligenceText.SetText(net.intelligence.Value.ToString());
            if (faithText != null)       faithText.SetText(net.faith.Value.ToString());
        }

        // ── 파생 자원 ──────────────────────────────────────────────────

        private void RefreshResources()
        {
            var net   = Player.playerNetworkManager;
            var stats = Player.characterStatsManager;

            maxHPText?.SetText(
                stats.CalculateHealthBasedOnVitalityLevel(net.vigor.Value).ToString());
            maxFPText?.SetText(
                stats.CalculateFocusPointsBasedOnMindLevel(net.mind.Value).ToString());
            maxStaminaText?.SetText(
                stats.CalculateStaminaBasedOnEnduranceLevel(net.endurance.Value).ToString());
        }

        // ── 공격력 ─────────────────────────────────────────────────────

        private void RefreshAttack()
        {
            var inv = Player.playerInventoryManager;

            SetWeaponAttackUI(inv.currentRightHandWeapon,
                rightPhysicalAtkText, rightMagicAtkText,
                rightFireAtkText, rightLightningAtkText, rightHolyAtkText);

            SetWeaponAttackUI(inv.currentLeftHandWeapon,
                leftPhysicalAtkText, leftMagicAtkText,
                leftFireAtkText, leftLightningAtkText, leftHolyAtkText);
        }

        /// WeaponManager.SetWeaponDamage와 동일한 스케일링·페널티 로직으로 표시 수치 계산
        private void SetWeaponAttackUI(
            WeaponItem weapon,
            TextMeshProUGUI physText, TextMeshProUGUI magText,
            TextMeshProUGUI fireText, TextMeshProUGUI lightText, TextMeshProUGUI holyText)
        {
            if (weapon == null)
            {
                physText?.SetText("—"); magText?.SetText("—");
                fireText?.SetText("—"); lightText?.SetText("—"); holyText?.SetText("—");
                return;
            }

            var net   = Player.playerNetworkManager;
            int str   = net.strength.Value + net.strengthModifier.Value;
            int dex   = net.dexterity.Value;
            int intel = net.intelligence.Value;
            int faith = net.faith.Value;

            float physBonus = 0f, magBonus = 0f;

            switch (weapon.weaponClass)
            {
                case WeaponClass.StraightSword:
                case WeaponClass.Fist:
                    physBonus = StatScalingBonus(weapon.physicalDamage, str, 0.5f);
                    break;
                case WeaponClass.Spear:
                    physBonus = StatScalingBonus(weapon.physicalDamage, str, 0.3f)
                              + StatScalingBonus(weapon.physicalDamage, dex, 0.3f);
                    break;
                case WeaponClass.Bow:
                    physBonus = StatScalingBonus(weapon.physicalDamage, dex, 0.5f);
                    break;
                case WeaponClass.Staff:
                    magBonus = StatScalingBonus(weapon.magicDamage, intel, 0.5f);
                    break;
            }

            bool meetsReq = str >= weapon.strengthREQ
                         && dex >= weapon.dexREQ
                         && intel >= weapon.intREQ
                         && faith >= weapon.faithREQ;
            float penalty = meetsReq ? 1f : 0.5f;

            // 요구 스탯 초과 스케일링
            if (meetsReq)
            {
                switch (weapon.weaponClass)
                {
                    case WeaponClass.StraightSword:
                    case WeaponClass.Fist:
                        physBonus += ExcessScalingBonus(weapon.physicalDamage, str, weapon.strengthREQ);
                        break;
                    case WeaponClass.Spear:
                        physBonus += ExcessScalingBonus(weapon.physicalDamage, str, weapon.strengthREQ)
                                   + ExcessScalingBonus(weapon.physicalDamage, dex, weapon.dexREQ);
                        break;
                    case WeaponClass.Bow:
                        physBonus += ExcessScalingBonus(weapon.physicalDamage, dex, weapon.dexREQ);
                        break;
                    case WeaponClass.Staff:
                        magBonus += ExcessScalingBonus(weapon.magicDamage, intel, weapon.intREQ);
                        break;
                }
            }

            float magicEffectBonus = (meetsReq && weapon.bonusEffectType == WeaponBonusEffectType.Magic)
                ? weapon.bonusEffectAmount : 0f;

            physText?.SetText(Mathf.RoundToInt((weapon.physicalDamage + physBonus) * penalty).ToString());
            magText?.SetText(Mathf.RoundToInt((weapon.magicDamage + magBonus + magicEffectBonus) * penalty).ToString());
            fireText?.SetText(Mathf.RoundToInt(weapon.fireDamage      * penalty).ToString());
            lightText?.SetText(Mathf.RoundToInt(weapon.lightningDamage * penalty).ToString());
            holyText?.SetText(Mathf.RoundToInt(weapon.holyDamage       * penalty).ToString());
        }

        // ── 방어력 ─────────────────────────────────────────────────────

        private void RefreshDefense()
        {
            var stats = Player.playerStatsManager;
            stats.CalculateTotalArmorAbsorption();

            physicalDefText?.SetText(stats.armorPhysicalDamageAbsorption.ToString("F1"));
            magicDefText?.SetText(stats.armorMagicDamageAbsorption.ToString("F1"));
            fireDefText?.SetText(stats.armorFireDamageAbsorption.ToString("F1"));
            lightningDefText?.SetText(stats.armorLightningDamageAbsorption.ToString("F1"));
            holyDefText?.SetText(stats.armorHolyDamageAbsorption.ToString("F1"));
            poiseText?.SetText(stats.basePoiseDefense.ToString("F0"));

            immunityText?.SetText(stats.armorImmunity.ToString("F0"));
            robustnessText?.SetText(stats.armorRobustness.ToString("F0"));
            focusResistText?.SetText(stats.armorFocus.ToString("F0"));
            vitalityText?.SetText(stats.armorVitality.ToString("F0"));
        }

        // ── 공통 유틸 ──────────────────────────────────────────────────

        private static float StatScalingBonus(float baseDamage, int statValue, float scaleFactor)
        {
            float normalized = Mathf.Clamp01(statValue / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * scaleFactor;
        }

        private static float ExcessScalingBonus(float baseDamage, int statValue, int requirement)
        {
            int excess = Mathf.Max(0, statValue - requirement);
            float normalized = Mathf.Clamp01(excess / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * 0.15f;
        }
    }
}
