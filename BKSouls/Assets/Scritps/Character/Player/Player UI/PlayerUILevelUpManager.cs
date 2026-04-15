using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BK
{
    public class PlayerUILevelUpManager : PlayerUIMenu
    {
        [Header("Levels")]
        [SerializeField] int[] playerLevels = new int[100];
        [SerializeField] int baseLevelCost = 83;
        [SerializeField] int totalLevelUpCost = 0;

        [Header("Character Stats")]
        [SerializeField] TextMeshProUGUI characterLevelText;
        [SerializeField] TextMeshProUGUI runesHeldText;
        [SerializeField] TextMeshProUGUI runesNeededText;
        [SerializeField] TextMeshProUGUI vigorLevelText;
        [SerializeField] TextMeshProUGUI mindLevelText;
        [SerializeField] TextMeshProUGUI enduranceLevelText;
        [SerializeField] TextMeshProUGUI strengthLevelText;
        [SerializeField] TextMeshProUGUI dexterityLevelText;
        [SerializeField] TextMeshProUGUI intelligenceLevelText;
        [SerializeField] TextMeshProUGUI faithLevelText;

        [Header("Projected Character Stats")]
        [SerializeField] TextMeshProUGUI projectedCharacterLevelText;
        [SerializeField] TextMeshProUGUI projectedRunesHeldText;
        [SerializeField] TextMeshProUGUI projectedVigorLevelText;
        [SerializeField] TextMeshProUGUI projectedMindLevelText;
        [SerializeField] TextMeshProUGUI projectedEnduranceLevelText;
        [SerializeField] TextMeshProUGUI projectedStrengthLevelText;
        [SerializeField] TextMeshProUGUI projectedDexterityLevelText;
        [SerializeField] TextMeshProUGUI projectedIntelligenceLevelText;
        [SerializeField] TextMeshProUGUI projectedFaithLevelText;

        [Header("Derived Stat Preview")]
        [SerializeField] TextMeshProUGUI maxHPPreviewText;
        [SerializeField] TextMeshProUGUI maxFPPreviewText;
        [SerializeField] TextMeshProUGUI maxStaminaPreviewText;

        [Header("Attack Preview (Right Hand)")]
        [SerializeField] TextMeshProUGUI rightPhysAtkPreviewText;
        [SerializeField] TextMeshProUGUI rightMagAtkPreviewText;

        [Header("Attack Preview (Left Hand)")]
        [SerializeField] TextMeshProUGUI leftPhysAtkPreviewText;
        [SerializeField] TextMeshProUGUI leftMagAtkPreviewText;

        [Header("Defense Display (Armor Based)")]
        [SerializeField] TextMeshProUGUI physDefDisplayText;
        [SerializeField] TextMeshProUGUI magDefDisplayText;
        [SerializeField] TextMeshProUGUI fireDefDisplayText;
        [SerializeField] TextMeshProUGUI lightningDefDisplayText;
        [SerializeField] TextMeshProUGUI holyDefDisplayText;
        [SerializeField] TextMeshProUGUI poiseDisplayText;

        [Header("Sliders")]
        public CharacterAttribute currentSelectedAttribute;
        public Slider vigorSlider;
        public Slider mindSlider;
        public Slider enduranceSlider;
        public Slider strengthSlider;
        public Slider dexteritySlider;
        public Slider intelligenceSlider;
        public Slider faithSlider;

        [Header("Buttons")]
        [SerializeField] Button confirmLevelsButton;

        private void Awake()
        {
            SetAllLevelsCost();
        }

        public override void OpenMenu()
        {
            base.OpenMenu();

            SetCurrentStats();
        }

        //  THIS IS CALLED WHEN OPENING THE MENU
        private void SetCurrentStats()
        {
            //  CHARACTER LEVEL
            characterLevelText.text = GUIController.Instance.localPlayer.characterStatsManager.CalculateCharacterLevelBasedOnAttributes().ToString();
            projectedCharacterLevelText.text = GUIController.Instance.localPlayer.characterStatsManager.CalculateCharacterLevelBasedOnAttributes().ToString();

            //  RUNES
            runesHeldText.text = GUIController.Instance.localPlayer.playerStatsManager.runes.ToString();
            projectedRunesHeldText.text = GUIController.Instance.localPlayer.playerStatsManager.runes.ToString();
            runesNeededText.text = "0";

            //  ATTRIBUTES
            vigorLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.vigor.Value.ToString();
            projectedVigorLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.vigor.Value.ToString();
            vigorSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.vigor.Value;

            mindLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.mind.Value.ToString();
            projectedMindLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.mind.Value.ToString();
            mindSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.mind.Value;

            enduranceLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.endurance.Value.ToString();
            projectedEnduranceLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.endurance.Value.ToString();
            enduranceSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.endurance.Value;

            strengthLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.strength.Value.ToString();
            projectedStrengthLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.strength.Value.ToString();
            strengthSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.strength.Value;

            dexterityLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.dexterity.Value.ToString();
            projectedDexterityLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.dexterity.Value.ToString();
            dexteritySlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.dexterity.Value;

            intelligenceLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.intelligence.Value.ToString();
            projectedIntelligenceLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.intelligence.Value.ToString();
            intelligenceSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.intelligence.Value;

            faithLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.faith.Value.ToString();
            projectedFaithLevelText.text = GUIController.Instance.localPlayer.playerNetworkManager.faith.Value.ToString();
            faithSlider.minValue = GUIController.Instance.localPlayer.playerNetworkManager.faith.Value;

            vigorSlider.Select();
            vigorSlider.OnSelect(null);

            RefreshDerivedStatPreviews();
            RefreshAttackPreview();
            RefreshDefenseDisplay();
        }

        //  THIS IS CALLED EVERY TIME A LEVEL SLIDER IS CHANGED
        public void UpdateSliderBasedOnCurrentlySelectedAttribute()
        {
            PlayerManager player = GUIController.Instance.localPlayer;

            switch (currentSelectedAttribute)
            {
                case CharacterAttribute.Vigor:
                    projectedVigorLevelText.text = vigorSlider.value.ToString();
                    break;
                case CharacterAttribute.Mind:
                    projectedMindLevelText.text = mindSlider.value.ToString();
                    break;
                case CharacterAttribute.Endurance:
                    projectedEnduranceLevelText.text = enduranceSlider.value.ToString();
                    break;
                case CharacterAttribute.Strength:
                    projectedStrengthLevelText.text = strengthSlider.value.ToString();
                    break;
                case CharacterAttribute.Dexterity:
                    projectedDexterityLevelText.text = dexteritySlider.value.ToString();
                    break;
                case CharacterAttribute.Intelligence:
                    projectedIntelligenceLevelText.text = intelligenceSlider.value.ToString();
                    break;
                case CharacterAttribute.Faith:
                    projectedFaithLevelText.text = faithSlider.value.ToString();
                    break;
                default:
                    break;
            }

            //  PASSES OUR CURRENT LEVEL AND OUR PROJECTED LEVEL TO SET OUR COST FOR LEVELING UP
            CalculateLevelCost(
                player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(),
                player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(true));

            projectedCharacterLevelText.text = player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(true).ToString();
            runesNeededText.text = totalLevelUpCost.ToString();

            //  CHECK COST
            if (totalLevelUpCost > player.playerStatsManager.runes)
            {
                //  DISABLE CONFIRM BUTTON AND RESET SLIDERS TO CURRENT STATS
                confirmLevelsButton.interactable = false;
                ResetSliders();
            }
            else
            {
                confirmLevelsButton.interactable = true;
            }

            //  CHANGES PROJECTED STATS TEXT COLORS TO REFLECT FEED BACK DEPENDING ON IF WE CAN AFFORD THE LEVELS OR NOT
            ChangeTextColorsDependingOnCosts();
            RefreshDerivedStatPreviews();
            RefreshAttackPreview();
            RefreshDefenseDisplay();
        }

        public void ConfirmLevels()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            var net = player.playerNetworkManager;

            //  DEDUCT COST FROM TOTAL RUNES
            player.playerStatsManager.runes -= totalLevelUpCost;

            //  SET NEW STATS
            net.vigor.Value      = Mathf.RoundToInt(vigorSlider.value);
            net.mind.Value       = Mathf.RoundToInt(mindSlider.value);
            net.endurance.Value  = Mathf.RoundToInt(enduranceSlider.value);
            net.strength.Value   = Mathf.RoundToInt(strengthSlider.value);
            net.dexterity.Value  = Mathf.RoundToInt(dexteritySlider.value);
            net.intelligence.Value = Mathf.RoundToInt(intelligenceSlider.value);
            net.faith.Value      = Mathf.RoundToInt(faithSlider.value);

            //  OnValueChanged 콜백 타이밍에 의존하지 않고 명시적으로 파생 자원 갱신
            net.SetNewMaxHealthValue(0, net.vigor.Value);
            net.SetNewMaxStaminaValue(0, net.endurance.Value);
            net.SetNewMaxFocusPointsValue(0, net.mind.Value);

            SetCurrentStats();
            ChangeTextColorsDependingOnCosts();

            //  SAVE GAME AFTER SETTING STATS
            WorldSaveGameManager.Instance.SaveGame();
        }

        // 모든 슬라이더를 현재 스탯(minValue)으로 되돌리고 UI를 초기 상태로 리셋
        public void ResetSliders()
        {
            vigorSlider.value        = vigorSlider.minValue;
            mindSlider.value         = mindSlider.minValue;
            enduranceSlider.value    = enduranceSlider.minValue;
            strengthSlider.value     = strengthSlider.minValue;
            dexteritySlider.value    = dexteritySlider.minValue;
            intelligenceSlider.value = intelligenceSlider.minValue;
            faithSlider.value        = faithSlider.minValue;

            totalLevelUpCost = 0;
            runesNeededText.text = "0";

            PlayerManager player = GUIController.Instance.localPlayer;
            projectedRunesHeldText.text  = player.playerStatsManager.runes.ToString();
            projectedRunesHeldText.color = Color.white;
            runesNeededText.color        = Color.white;

            confirmLevelsButton.interactable = false;

            RefreshDerivedStatPreviews();
            RefreshAttackPreview();
            RefreshDefenseDisplay();
            ChangeTextColorsDependingOnCosts();
        }

        private void SetAllLevelsCost()
        {
            for (int i = 0; i < playerLevels.Length; i++)
            {
                //  LEVEL 0 HAS NO COST
                if (i == 0)
                    continue;

                playerLevels[i] = baseLevelCost + (50 * i);
            }
        }

        private void CalculateLevelCost(int currentLevel, int projectedLevel)
        {
            int totalCost = 0;

            //  WE DONT WANT TO CHARGE FOR LEVELS WE ALREADY PAID FOR
            //  EX, IF YOU ARE LEVEL 21 WE DONT ADD THE COST OF THE FIRST 21 LEVELS
            for (int i = 0; i < projectedLevel; i++)
            {
                if (i < currentLevel)
                    continue;

                //  THIS IS A SAFEGUARD TO STOP ADDING COST IF THE PLAYERS LEVEL SOME HOW EXCEEDS THE SIZE OF THE ARRAY WE HAVE CREATED
                //  YOU CAN OPTIONALLY ADD A CHECK FOR A TOTAL LEVEL AND PREVENT ADVANCES BEYOND THAT OR ALLOW THE MAX VALUE OF YOUR SLIDERS TO DETERMINE THE MAX LEVEL (LIKE ELDEN/SOULS)
                if (i > playerLevels.Length)
                    continue;

                totalCost += playerLevels[i];
            }

            totalLevelUpCost = totalCost;

            projectedRunesHeldText.text = (GUIController.Instance.localPlayer.playerStatsManager.runes - totalCost).ToString();

            if (totalCost > GUIController.Instance.localPlayer.playerStatsManager.runes)
            {
                projectedRunesHeldText.color = Color.red;
            }
            else
            {
                projectedRunesHeldText.color = Color.white;
            }
        }

        //  THIS WILL CHANGE THE COLORS OF THE PROJECTED LEVELS...
        //  TO RED (IF YOU CANT AFFORD AND THE STATE IS HIGHER THAN THE CURRENT LEVEL) - YOU COULD OPTIONALLY TURN ALL PROJECTED STATS RED IF YOU ENTER A STATE WHERE YOU CANT AFFORD
        //  TO BLUE (IF THE STAT IS HIGHER CAN YOU CAN AFFORD IT)
        //  TO WHITE (IF THE STAT IS UNCHANGED)
        private void ChangeTextColorsDependingOnCosts()
        {
            PlayerManager player = GUIController.Instance.localPlayer;

            int projectedVigorLevel = Mathf.RoundToInt(vigorSlider.value);
            int projectedMindLevel = Mathf.RoundToInt(mindSlider.value);
            int projectedEnduranceLevel = Mathf.RoundToInt(enduranceSlider.value);
            int projectedStrengthLevel = Mathf.RoundToInt(strengthSlider.value);
            int projectedDexterityLevel = Mathf.RoundToInt(dexteritySlider.value);
            int projectedIntelligenceLevel = Mathf.RoundToInt(intelligenceSlider.value);
            int projectedFaithLevel = Mathf.RoundToInt(faithSlider.value);

            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedVigorLevelText, player.playerNetworkManager.vigor.Value, projectedVigorLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedMindLevelText, player.playerNetworkManager.mind.Value, projectedMindLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedEnduranceLevelText, player.playerNetworkManager.endurance.Value, projectedEnduranceLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedStrengthLevelText, player.playerNetworkManager.strength.Value, projectedStrengthLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedDexterityLevelText, player.playerNetworkManager.dexterity.Value, projectedDexterityLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedIntelligenceLevelText, player.playerNetworkManager.intelligence.Value, projectedIntelligenceLevel);
            ChangeTextFieldToSpecificColorBasedOnStat(player, projectedFaithLevelText, player.playerNetworkManager.faith.Value, projectedFaithLevel);

            int projectedPlayerLevel = player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(true);
            int playerLevel = player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes();

            if (projectedPlayerLevel == playerLevel)
            {
                projectedCharacterLevelText.color = Color.white;
                projectedRunesHeldText.color = Color.white;
                runesNeededText.color = Color.white;
            }

            //  WE CAN AFFORD IT!
            if (totalLevelUpCost <= player.playerStatsManager.runes)
            {
                runesNeededText.color = Color.white;

                if (projectedPlayerLevel > playerLevel)
                {
                    projectedRunesHeldText.color = Color.red;
                    projectedCharacterLevelText.color = Color.blue;
                }
            }
            else
            {
                runesNeededText.color = Color.red;

                if (projectedPlayerLevel > playerLevel)
                    projectedCharacterLevelText.color = Color.red;
            }
        }

        // ── 파생 스탯 미리보기 ─────────────────────────────────────────
        // 슬라이더 현재값을 기준으로 HP/FP/Stamina의 변화를 "현재 > 예상" 형식으로 표시
        private void RefreshDerivedStatPreviews()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            var stats = player.characterStatsManager;
            var net   = player.playerNetworkManager;

            int curVigor     = net.vigor.Value;
            int curMind      = net.mind.Value;
            int curEndurance = net.endurance.Value;

            int projVigor     = Mathf.RoundToInt(vigorSlider.value);
            int projMind      = Mathf.RoundToInt(mindSlider.value);
            int projEndurance = Mathf.RoundToInt(enduranceSlider.value);

            bool canAfford = totalLevelUpCost <= player.playerStatsManager.runes;

            SetDerivedPreview(maxHPPreviewText,
                stats.CalculateHealthBasedOnVitalityLevel(curVigor),
                stats.CalculateHealthBasedOnVitalityLevel(projVigor),
                canAfford);

            SetDerivedPreview(maxFPPreviewText,
                stats.CalculateFocusPointsBasedOnMindLevel(curMind),
                stats.CalculateFocusPointsBasedOnMindLevel(projMind),
                canAfford);

            SetDerivedPreview(maxStaminaPreviewText,
                stats.CalculateStaminaBasedOnEnduranceLevel(curEndurance),
                stats.CalculateStaminaBasedOnEnduranceLevel(projEndurance),
                canAfford);
        }

        // ── 공격력 미리보기 ────────────────────────────────────────────
        // STR/DEX/INT 슬라이더 값(투영) 기준으로 무기 공격력 변화를 표시
        private void RefreshAttackPreview()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            var net = player.playerNetworkManager;
            bool canAfford = totalLevelUpCost <= player.playerStatsManager.runes;

            // 현재 스탯
            int curStr   = net.strength.Value + net.strengthModifier.Value;
            int curDex   = net.dexterity.Value;
            int curIntel = net.intelligence.Value;
            int curFaith = net.faith.Value;

            // 투영(예정) 스탯
            int projStr   = Mathf.RoundToInt(strengthSlider.value) + net.strengthModifier.Value;
            int projDex   = Mathf.RoundToInt(dexteritySlider.value);
            int projIntel = Mathf.RoundToInt(intelligenceSlider.value);
            int projFaith = Mathf.RoundToInt(faithSlider.value);

            var inv = player.playerInventoryManager;

            SetWeaponAttackPreview(inv.currentRightHandWeapon,
                curStr, curDex, curIntel, curFaith,
                projStr, projDex, projIntel, projFaith,
                rightPhysAtkPreviewText, rightMagAtkPreviewText, canAfford);

            SetWeaponAttackPreview(inv.currentLeftHandWeapon,
                curStr, curDex, curIntel, curFaith,
                projStr, projDex, projIntel, projFaith,
                leftPhysAtkPreviewText, leftMagAtkPreviewText, canAfford);
        }

        private void SetWeaponAttackPreview(
            WeaponItem weapon,
            int curStr, int curDex, int curIntel, int curFaith,
            int projStr, int projDex, int projIntel, int projFaith,
            TextMeshProUGUI physField, TextMeshProUGUI magField,
            bool canAfford)
        {
            if (weapon == null)
            {
                physField?.SetText("—");
                magField?.SetText("—");
                return;
            }

            var (curPhys, curMag)   = CalcWeaponAttack(weapon, curStr,  curDex,  curIntel,  curFaith);
            var (projPhys, projMag) = CalcWeaponAttack(weapon, projStr, projDex, projIntel, projFaith);

            SetDerivedPreview(physField, curPhys, projPhys, canAfford);
            SetDerivedPreview(magField,  curMag,  projMag,  canAfford);
        }

        // WeaponManager/PlayerUICharacterMenuManager와 동일한 스케일링 로직
        private static (int phys, int mag) CalcWeaponAttack(WeaponItem weapon, int str, int dex, int intel, int faith)
        {
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

            bool meetsReq = str   >= weapon.strengthREQ
                         && dex   >= weapon.dexREQ
                         && intel >= weapon.intREQ
                         && faith >= weapon.faithREQ;
            float penalty = meetsReq ? 1f : 0.5f;

            int phys = Mathf.RoundToInt((weapon.physicalDamage + physBonus) * penalty);
            int mag  = Mathf.RoundToInt((weapon.magicDamage  + magBonus)  * penalty);
            return (phys, mag);
        }

        private static float StatScalingBonus(float baseDamage, int statValue, float scaleFactor)
        {
            float normalized = Mathf.Clamp01(statValue / 99f);
            float curve = 1f - Mathf.Pow(1f - normalized, 2f);
            return baseDamage * curve * scaleFactor;
        }

        // ── 방어력 표시 (스탯 무관 / 방어구 기반) ────────────────────────
        private void RefreshDefenseDisplay()
        {
            var stats = GUIController.Instance.localPlayer.playerStatsManager;
            stats.CalculateTotalArmorAbsorption();

            physDefDisplayText?.SetText(stats.armorPhysicalDamageAbsorption.ToString("F1"));
            magDefDisplayText?.SetText(stats.armorMagicDamageAbsorption.ToString("F1"));
            fireDefDisplayText?.SetText(stats.armorFireDamageAbsorption.ToString("F1"));
            lightningDefDisplayText?.SetText(stats.armorLightningDamageAbsorption.ToString("F1"));
            holyDefDisplayText?.SetText(stats.armorHolyDamageAbsorption.ToString("F1"));
            poiseDisplayText?.SetText(stats.basePoiseDefense.ToString("F0"));
        }

        // current == projected → 그냥 숫자 (흰색)
        // current  < projected → "current > projected" (파란색 or 빨간색)
        private static void SetDerivedPreview(TextMeshProUGUI field, int current, int projected, bool canAfford)
        {
            if (field == null) return;

            if (projected == current)
            {
                field.text  = current.ToString();
                field.color = Color.white;
            }
            else
            {
                field.text  = $"{current} > {projected}";
                field.color = canAfford ? Color.blue : Color.red;
            }
        }

        private void ChangeTextFieldToSpecificColorBasedOnStat(PlayerManager player, TextMeshProUGUI textField, int stat, int projectedStat)
        {
            if (projectedStat == stat)
                textField.color = Color.white;

            //  WE CAN AFFORD IT!
            if (totalLevelUpCost <= player.playerStatsManager.runes)
            {
                //  IF OUR PROJECTED STAT IS HIGHER, GIVE THE PLAYER VISUAL FEEDBACK BY CHANGING ITS COLOR INDICATING ITS NEW POTENTIAL
                if (projectedStat > stat)
                {
                    textField.color = Color.blue;
                }
                //  IF OUR PROJECTED STAT IS THE SAME, KEEP THE TEXT COLOR AS DEFAULT
                else
                {
                    textField.color = Color.white;
                }
            }
            //  WE CANT AFFORD IT
            else
            {
                //  IF OUR PROJECTED STAT IS HIGHER, GIVE THE PLAYER VISUAL FEEDBACK BY CHANGING ITS COLOR INDICATING ITS NEW POTENTIAL
                if (projectedStat > stat)
                {
                    textField.color = Color.red;
                }
                //  IF OUR PROJECTED STAT IS THE SAME, KEEP THE TEXT COLOR AS DEFAULT
                else
                {
                    textField.color = Color.white;
                }
            }
        }
    }
}
