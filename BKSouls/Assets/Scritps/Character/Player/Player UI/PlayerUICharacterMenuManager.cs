using TMPro;
using UnityEngine;

namespace BK
{
    public class PlayerUICharacterMenuManager : PlayerUIMenu
    {
        private PlayerManager Player => GUIController.Instance.localPlayer;

        [Header("Derived Resources")]
        [SerializeField] private TextMeshProUGUI maxHPText;
        [SerializeField] private TextMeshProUGUI maxFPText;
        [SerializeField] private TextMeshProUGUI maxStaminaText;

        [Header("Right Hand Attack")]
        [SerializeField] private TextMeshProUGUI rightPhysicalAtkText;
        [SerializeField] private TextMeshProUGUI rightMagicAtkText;
        [SerializeField] private TextMeshProUGUI rightFireAtkText;
        [SerializeField] private TextMeshProUGUI rightLightningAtkText;
        [SerializeField] private TextMeshProUGUI rightHolyAtkText;

        [Header("Left Hand Attack")]
        [SerializeField] private TextMeshProUGUI leftPhysicalAtkText;
        [SerializeField] private TextMeshProUGUI leftMagicAtkText;
        [SerializeField] private TextMeshProUGUI leftFireAtkText;
        [SerializeField] private TextMeshProUGUI leftLightningAtkText;
        [SerializeField] private TextMeshProUGUI leftHolyAtkText;

        [Header("Defense")]
        [SerializeField] private TextMeshProUGUI physicalDefText;
        [SerializeField] private TextMeshProUGUI magicDefText;
        [SerializeField] private TextMeshProUGUI fireDefText;
        [SerializeField] private TextMeshProUGUI lightningDefText;
        [SerializeField] private TextMeshProUGUI holyDefText;
        [SerializeField] private TextMeshProUGUI poiseText;

        [Header("Attributes")]
        [SerializeField] private TextMeshProUGUI strengthText;
        [SerializeField] private TextMeshProUGUI dexterityText;
        [SerializeField] private TextMeshProUGUI intelligenceText;
        [SerializeField] private TextMeshProUGUI faithText;

        [Header("Status Resistance")]
        [SerializeField] private TextMeshProUGUI immunityText;
        [SerializeField] private TextMeshProUGUI robustnessText;
        [SerializeField] private TextMeshProUGUI focusResistText;
        [SerializeField] private TextMeshProUGUI vitalityText;

        public override void OpenMenu()
        {
            base.OpenMenu();
            Refresh();
        }

        public void Refresh()
        {
            if (Player == null) return;

            RefreshAttributes();
            RefreshResources();
            RefreshAttack();
            RefreshDefense();
        }

        private void RefreshAttributes()
        {
            var net = Player.playerNetworkManager;
            strengthText?.SetText((net.strength.Value + net.strengthModifier.Value).ToString());
            dexterityText?.SetText(net.dexterity.Value.ToString());
            intelligenceText?.SetText(net.intelligence.Value.ToString());
            faithText?.SetText(net.faith.Value.ToString());
        }

        private void RefreshResources()
        {
            var net = Player.playerNetworkManager;
            var stats = Player.characterStatsManager;

            maxHPText?.SetText(stats.CalculateHealthBasedOnVitalityLevel(net.vigor.Value).ToString());
            maxFPText?.SetText(stats.CalculateFocusPointsBasedOnMindLevel(net.mind.Value).ToString());
            maxStaminaText?.SetText(stats.CalculateStaminaBasedOnEnduranceLevel(net.endurance.Value).ToString());
        }

        private void RefreshAttack()
        {
            var inv = Player.playerInventoryManager;

            SetWeaponAttackUI(
                inv.currentRightHandWeapon,
                rightPhysicalAtkText,
                rightMagicAtkText,
                rightFireAtkText,
                rightLightningAtkText,
                rightHolyAtkText);

            SetWeaponAttackUI(
                inv.currentLeftHandWeapon,
                leftPhysicalAtkText,
                leftMagicAtkText,
                leftFireAtkText,
                leftLightningAtkText,
                leftHolyAtkText);
        }

        private void SetWeaponAttackUI(
            WeaponItem weapon,
            TextMeshProUGUI physText,
            TextMeshProUGUI magText,
            TextMeshProUGUI fireText,
            TextMeshProUGUI lightText,
            TextMeshProUGUI holyText)
        {
            if (weapon == null)
            {
                physText?.SetText("--");
                magText?.SetText("--");
                fireText?.SetText("--");
                lightText?.SetText("--");
                holyText?.SetText("--");
                return;
            }

            var net = Player.playerNetworkManager;
            WeaponManager.CalculateScaledWeaponDamage(
                weapon,
                net.strength.Value + net.strengthModifier.Value,
                net.dexterity.Value,
                net.intelligence.Value,
                net.faith.Value,
                out float physicalDamage,
                out float magicDamage,
                out float fireDamage,
                out float lightningDamage,
                out float holyDamage);

            physText?.SetText(Mathf.RoundToInt(physicalDamage).ToString());
            magText?.SetText(Mathf.RoundToInt(magicDamage).ToString());
            fireText?.SetText(Mathf.RoundToInt(fireDamage).ToString());
            lightText?.SetText(Mathf.RoundToInt(lightningDamage).ToString());
            holyText?.SetText(Mathf.RoundToInt(holyDamage).ToString());
        }

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
    }
}
