using System.Collections.Generic;
using BK.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK
{
    public class PlayerUIBaseStatUpgradeManager : PlayerUIMenu
    {
        [Header("Cost Config")]
        [SerializeField] private List<int> startingRuneUpgradeCosts = new() { 500, 1000, 1500, 2200, 3000, 4000, 5200, 6600, 8200, 10000 };
        [SerializeField] private List<int> vigorCoefficientUpgradeCosts = new() { 1500, 4000, 8000 };
        [SerializeField] private List<int> mindCoefficientUpgradeCosts = new() { 1500, 4000, 8000 };
        [SerializeField] private List<int> enduranceCoefficientUpgradeCosts = new() { 1500, 4000, 8000 };
        [SerializeField] private List<int> healthFlaskUpgradeCosts = new() { 1000, 2000, 3500, 5500, 8000 };
        [SerializeField] private List<int> healthFlaskHealUpgradeCosts = new() { 1200, 2500, 4000, 6000, 9000 };
        [SerializeField] private List<int> focusPointFlaskUpgradeCosts = new() { 1500, 3000, 5000, 7500, 11000 };
        [SerializeField] private List<int> focusPointFlaskHealUpgradeCosts = new() { 1200, 2500, 4000, 6000, 9000 };

        [Header("Balance")]
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("Starting Runes")]
        [SerializeField] private TextMeshProUGUI startingRuneLevelText;
        [SerializeField] private TextMeshProUGUI startingRuneCurrentText;
        [SerializeField] private TextMeshProUGUI startingRuneNextText;
        [SerializeField] private TextMeshProUGUI startingRuneCostText;

        [Header("Health Flasks")]
        [SerializeField] private TextMeshProUGUI healthFlaskLevelText;
        [SerializeField] private TextMeshProUGUI healthFlaskCurrentText;
        [SerializeField] private TextMeshProUGUI healthFlaskNextText;
        [SerializeField] private TextMeshProUGUI healthFlaskCostText;

        [Header("Health Flask Heal")]
        [SerializeField] private TextMeshProUGUI healthFlaskHealLevelText;
        [SerializeField] private TextMeshProUGUI healthFlaskHealCurrentText;
        [SerializeField] private TextMeshProUGUI healthFlaskHealNextText;
        [SerializeField] private TextMeshProUGUI healthFlaskHealCostText;

        [Header("Focus Point Flasks")]
        [SerializeField] private TextMeshProUGUI focusPointFlaskLevelText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskCurrentText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskNextText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskCostText;

        [Header("Focus Point Flask Heal")]
        [SerializeField] private TextMeshProUGUI focusPointFlaskHealLevelText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskHealCurrentText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskHealNextText;
        [SerializeField] private TextMeshProUGUI focusPointFlaskHealCostText;

        [Header("Vigor Coefficient")]
        [SerializeField] private TextMeshProUGUI vigorCoefficientLevelText;
        [SerializeField] private TextMeshProUGUI vigorCoefficientCurrentText;
        [SerializeField] private TextMeshProUGUI vigorCoefficientNextText;
        [SerializeField] private TextMeshProUGUI vigorCoefficientCostText;

        [Header("Mind Coefficient")]
        [SerializeField] private TextMeshProUGUI mindCoefficientLevelText;
        [SerializeField] private TextMeshProUGUI mindCoefficientCurrentText;
        [SerializeField] private TextMeshProUGUI mindCoefficientNextText;
        [SerializeField] private TextMeshProUGUI mindCoefficientCostText;

        [Header("Endurance Coefficient")]
        [SerializeField] private TextMeshProUGUI enduranceCoefficientLevelText;
        [SerializeField] private TextMeshProUGUI enduranceCoefficientCurrentText;
        [SerializeField] private TextMeshProUGUI enduranceCoefficientNextText;
        [SerializeField] private TextMeshProUGUI enduranceCoefficientCostText;

        [Header("Buttons")]
        [SerializeField] private Button startingRuneUpgradeButton;
        [SerializeField] private Button healthFlaskUpgradeButton;
        [SerializeField] private Button healthFlaskHealUpgradeButton;
        [SerializeField] private Button focusPointFlaskUpgradeButton;
        [SerializeField] private Button focusPointFlaskHealUpgradeButton;
        [SerializeField] private Button vigorCoefficientUpgradeButton;
        [SerializeField] private Button mindCoefficientUpgradeButton;
        [SerializeField] private Button enduranceCoefficientUpgradeButton;
        [SerializeField] private Button closeButton;

        private bool _isSubscribedToBalance;

        private void Awake()
        {
            startingRuneUpgradeButton?.onClick.AddListener(TryUpgradeStartingRunes);
            healthFlaskUpgradeButton?.onClick.AddListener(TryUpgradeHealthFlasks);
            healthFlaskHealUpgradeButton?.onClick.AddListener(TryUpgradeHealthFlaskHeal);
            focusPointFlaskUpgradeButton?.onClick.AddListener(TryUpgradeFocusPointFlasks);
            focusPointFlaskHealUpgradeButton?.onClick.AddListener(TryUpgradeFocusPointFlaskHeal);
            vigorCoefficientUpgradeButton?.onClick.AddListener(TryUpgradeVigorCoefficient);
            mindCoefficientUpgradeButton?.onClick.AddListener(TryUpgradeMindCoefficient);
            enduranceCoefficientUpgradeButton?.onClick.AddListener(TryUpgradeEnduranceCoefficient);
            closeButton?.onClick.AddListener(CloseMenu);
        }

        public override void OpenMenu()
        {
            base.OpenMenu();
            SubscribeBalance();
            Refresh();
        }

        public override void CloseMenu()
        {
            UnsubscribeBalance();
            base.CloseMenu();
        }

        public void TryUpgradeStartingRunes()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.startingRuneBonusLevel >= CharacterSaveData.MaxStartingRuneBonusLevel)
                return;

            int cost = GetUpgradeCost(startingRuneUpgradeCosts, data.startingRuneBonusLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.startingRuneBonusLevel++;
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeHealthFlasks()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.healthFlaskBonusLevel >= CharacterSaveData.MaxHealthFlaskBonusLevel)
                return;

            int cost = GetUpgradeCost(healthFlaskUpgradeCosts, data.healthFlaskBonusLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.healthFlaskBonusLevel++;
            WorldSaveGameManager.Instance.ResetFlasksToDefaultCharges();
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeHealthFlaskHeal()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.healthFlaskHealBonusLevel >= CharacterSaveData.MaxHealthFlaskHealBonusLevel)
                return;

            int cost = GetUpgradeCost(healthFlaskHealUpgradeCosts, data.healthFlaskHealBonusLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.healthFlaskHealBonusLevel++;
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeFocusPointFlasks()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.focusPointFlaskBonusLevel >= CharacterSaveData.MaxFocusPointFlaskBonusLevel)
                return;

            int cost = GetUpgradeCost(focusPointFlaskUpgradeCosts, data.focusPointFlaskBonusLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.focusPointFlaskBonusLevel++;
            WorldSaveGameManager.Instance.ResetFlasksToDefaultCharges();
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeFocusPointFlaskHeal()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.focusPointFlaskHealBonusLevel >= CharacterSaveData.MaxFocusPointFlaskHealBonusLevel)
                return;

            int cost = GetUpgradeCost(focusPointFlaskHealUpgradeCosts, data.focusPointFlaskHealBonusLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.focusPointFlaskHealBonusLevel++;
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeVigorCoefficient()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.vigorCoefficientLevel >= CharacterSaveData.MaxVigorCoefficientLevel)
                return;

            int cost = GetUpgradeCost(vigorCoefficientUpgradeCosts, data.vigorCoefficientLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.vigorCoefficientLevel++;
            RefreshPlayerHealthAfterVigorCoefficientUpgrade();
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeMindCoefficient()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.mindCoefficientLevel >= CharacterSaveData.MaxMindCoefficientLevel)
                return;

            int cost = GetUpgradeCost(mindCoefficientUpgradeCosts, data.mindCoefficientLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.mindCoefficientLevel++;
            RefreshPlayerFocusPointsAfterMindCoefficientUpgrade();
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        public void TryUpgradeEnduranceCoefficient()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null || data.enduranceCoefficientLevel >= CharacterSaveData.MaxEnduranceCoefficientLevel)
                return;

            int cost = GetUpgradeCost(enduranceCoefficientUpgradeCosts, data.enduranceCoefficientLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost))
                return;

            data.enduranceCoefficientLevel++;
            RefreshPlayerStaminaAfterEnduranceCoefficientUpgrade();
            WorldSaveGameManager.Instance.SaveGame();
            Refresh();
        }

        private void Refresh()
        {
            CharacterSaveData data = WorldSaveGameManager.Instance.currentCharacterData;
            if (data == null)
                return;

            int balance = WorldPlayerInventory.Instance != null ? WorldPlayerInventory.Instance.balance.Value : 0;
            if (balanceText != null)
                balanceText.text = balance.ToString();

            RefreshStartingRuneUI(data, balance);
            RefreshHealthFlaskUI(data, balance);
            RefreshHealthFlaskHealUI(data, balance);
            RefreshFocusPointFlaskUI(data, balance);
            RefreshFocusPointFlaskHealUI(data, balance);
            RefreshVigorCoefficientUI(data, balance);
            RefreshMindCoefficientUI(data, balance);
            RefreshEnduranceCoefficientUI(data, balance);
        }

        private void RefreshStartingRuneUI(CharacterSaveData data, int balance)
        {
            int level = Mathf.Clamp(data.startingRuneBonusLevel, 0, CharacterSaveData.MaxStartingRuneBonusLevel);
            int currentRunes = level * 1000;
            bool isMax = level >= CharacterSaveData.MaxStartingRuneBonusLevel;
            int nextRunes = isMax ? currentRunes : (level + 1) * 1000;
            int cost = isMax ? 0 : GetUpgradeCost(startingRuneUpgradeCosts, level);

            startingRuneLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            startingRuneCurrentText?.SetText(currentRunes.ToString());
            startingRuneNextText?.SetText(isMax ? "MAX" : nextRunes.ToString());
            startingRuneCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (startingRuneUpgradeButton != null)
                startingRuneUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshHealthFlaskUI(CharacterSaveData data, int balance)
        {
            int level = Mathf.Clamp(data.healthFlaskBonusLevel, 0, CharacterSaveData.MaxHealthFlaskBonusLevel);
            bool isMax = level >= CharacterSaveData.MaxHealthFlaskBonusLevel;
            int currentCharges = CharacterSaveData.DefaultHealthFlaskCharges + level;
            int nextCharges = isMax ? currentCharges : currentCharges + 1;
            int cost = isMax ? 0 : GetUpgradeCost(healthFlaskUpgradeCosts, level);

            healthFlaskLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            healthFlaskCurrentText?.SetText(currentCharges.ToString());
            healthFlaskNextText?.SetText(isMax ? "MAX" : nextCharges.ToString());
            healthFlaskCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (healthFlaskUpgradeButton != null)
                healthFlaskUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshHealthFlaskHealUI(CharacterSaveData data, int balance)
        {
            int level = Mathf.Clamp(data.healthFlaskHealBonusLevel, 0, CharacterSaveData.MaxHealthFlaskHealBonusLevel);
            bool isMax = level >= CharacterSaveData.MaxHealthFlaskHealBonusLevel;
            int currentBonus = level * CharacterSaveData.HealthFlaskHealBonusPerLevel;
            int nextBonus = isMax ? currentBonus : (level + 1) * CharacterSaveData.HealthFlaskHealBonusPerLevel;
            int cost = isMax ? 0 : GetUpgradeCost(healthFlaskHealUpgradeCosts, level);

            healthFlaskHealLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            healthFlaskHealCurrentText?.SetText($"+{currentBonus}");
            healthFlaskHealNextText?.SetText(isMax ? "MAX" : $"+{nextBonus}");
            healthFlaskHealCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (healthFlaskHealUpgradeButton != null)
                healthFlaskHealUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshFocusPointFlaskUI(CharacterSaveData data, int balance)
        {
            int level = Mathf.Clamp(data.focusPointFlaskBonusLevel, 0, CharacterSaveData.MaxFocusPointFlaskBonusLevel);
            bool isMax = level >= CharacterSaveData.MaxFocusPointFlaskBonusLevel;
            int currentCharges = CharacterSaveData.DefaultFocusPointFlaskCharges + level;
            int nextCharges = isMax ? currentCharges : currentCharges + 1;
            int cost = isMax ? 0 : GetUpgradeCost(focusPointFlaskUpgradeCosts, level);

            focusPointFlaskLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            focusPointFlaskCurrentText?.SetText(currentCharges.ToString());
            focusPointFlaskNextText?.SetText(isMax ? "MAX" : nextCharges.ToString());
            focusPointFlaskCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (focusPointFlaskUpgradeButton != null)
                focusPointFlaskUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshFocusPointFlaskHealUI(CharacterSaveData data, int balance)
        {
            int level = Mathf.Clamp(data.focusPointFlaskHealBonusLevel, 0, CharacterSaveData.MaxFocusPointFlaskHealBonusLevel);
            bool isMax = level >= CharacterSaveData.MaxFocusPointFlaskHealBonusLevel;
            int currentBonus = level * CharacterSaveData.FocusPointFlaskHealBonusPerLevel;
            int nextBonus = isMax ? currentBonus : (level + 1) * CharacterSaveData.FocusPointFlaskHealBonusPerLevel;
            int cost = isMax ? 0 : GetUpgradeCost(focusPointFlaskHealUpgradeCosts, level);

            focusPointFlaskHealLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            focusPointFlaskHealCurrentText?.SetText($"+{currentBonus}");
            focusPointFlaskHealNextText?.SetText(isMax ? "MAX" : $"+{nextBonus}");
            focusPointFlaskHealCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (focusPointFlaskHealUpgradeButton != null)
                focusPointFlaskHealUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshVigorCoefficientUI(CharacterSaveData data, int balance)
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            CharacterStatsManager stats = player != null ? player.characterStatsManager : null;

            int level = Mathf.Clamp(data.vigorCoefficientLevel, 0, CharacterSaveData.MaxVigorCoefficientLevel);
            bool isMax = level >= CharacterSaveData.MaxVigorCoefficientLevel;
            int currentCoefficient = stats != null ? stats.GetHealthPerVigorForUpgradeLevel(level) : 0;
            int nextCoefficient = stats != null ? stats.GetHealthPerVigorForUpgradeLevel(level + 1) : 0;
            int cost = isMax ? 0 : GetUpgradeCost(vigorCoefficientUpgradeCosts, level);

            vigorCoefficientLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            vigorCoefficientCurrentText?.SetText(currentCoefficient.ToString());
            vigorCoefficientNextText?.SetText(isMax ? "MAX" : nextCoefficient.ToString());
            vigorCoefficientCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (vigorCoefficientUpgradeButton != null)
                vigorCoefficientUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshMindCoefficientUI(CharacterSaveData data, int balance)
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            CharacterStatsManager stats = player != null ? player.characterStatsManager : null;

            int level = Mathf.Clamp(data.mindCoefficientLevel, 0, CharacterSaveData.MaxMindCoefficientLevel);
            bool isMax = level >= CharacterSaveData.MaxMindCoefficientLevel;
            int currentCoefficient = stats != null ? stats.GetFocusPointsPerMindForUpgradeLevel(level) : 0;
            int nextCoefficient = stats != null ? stats.GetFocusPointsPerMindForUpgradeLevel(level + 1) : 0;
            int cost = isMax ? 0 : GetUpgradeCost(mindCoefficientUpgradeCosts, level);

            mindCoefficientLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            mindCoefficientCurrentText?.SetText(currentCoefficient.ToString());
            mindCoefficientNextText?.SetText(isMax ? "MAX" : nextCoefficient.ToString());
            mindCoefficientCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (mindCoefficientUpgradeButton != null)
                mindCoefficientUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshEnduranceCoefficientUI(CharacterSaveData data, int balance)
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            CharacterStatsManager stats = player != null ? player.characterStatsManager : null;

            int level = Mathf.Clamp(data.enduranceCoefficientLevel, 0, CharacterSaveData.MaxEnduranceCoefficientLevel);
            bool isMax = level >= CharacterSaveData.MaxEnduranceCoefficientLevel;
            int currentCoefficient = stats != null ? stats.GetStaminaPerEnduranceForUpgradeLevel(level) : 0;
            int nextCoefficient = stats != null ? stats.GetStaminaPerEnduranceForUpgradeLevel(level + 1) : 0;
            int cost = isMax ? 0 : GetUpgradeCost(enduranceCoefficientUpgradeCosts, level);

            enduranceCoefficientLevelText?.SetText(isMax ? "MAX" : $"LV {level + 1}");
            enduranceCoefficientCurrentText?.SetText(currentCoefficient.ToString());
            enduranceCoefficientNextText?.SetText(isMax ? "MAX" : nextCoefficient.ToString());
            enduranceCoefficientCostText?.SetText(isMax ? "MAX" : cost.ToString());

            if (enduranceCoefficientUpgradeButton != null)
                enduranceCoefficientUpgradeButton.interactable = !isMax && balance >= cost;
        }

        private void RefreshPlayerHealthAfterVigorCoefficientUpgrade()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            if (player == null)
                return;

            var net = player.playerNetworkManager;
            net.maxHealth.Value = player.playerStatsManager.CalculateHealthBasedOnVitalityLevel(net.vigor.Value);
            net.currentHealth.Value = net.maxHealth.Value;
        }

        private void RefreshPlayerFocusPointsAfterMindCoefficientUpgrade()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            if (player == null)
                return;

            var net = player.playerNetworkManager;
            net.maxFocusPoints.Value = player.playerStatsManager.CalculateFocusPointsBasedOnMindLevel(net.mind.Value);
            net.currentFocusPoints.Value = net.maxFocusPoints.Value;
        }

        private void RefreshPlayerStaminaAfterEnduranceCoefficientUpgrade()
        {
            PlayerManager player = GUIController.Instance.localPlayer;
            if (player == null)
                return;

            var net = player.playerNetworkManager;
            net.maxStamina.Value = player.playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(net.endurance.Value);
            net.currentStamina.Value = net.maxStamina.Value;
        }

        private static int GetUpgradeCost(List<int> costs, int currentLevel)
        {
            if (costs == null || costs.Count == 0)
                return 0;

            if (currentLevel < costs.Count)
                return costs[currentLevel];

            return Mathf.RoundToInt(costs[costs.Count - 1] * Mathf.Pow(2f, currentLevel - costs.Count + 1));
        }

        private void SubscribeBalance()
        {
            if (_isSubscribedToBalance || WorldPlayerInventory.Instance == null)
                return;

            WorldPlayerInventory.Instance.balance.OnValueChanged += OnBalanceChanged;
            _isSubscribedToBalance = true;
        }

        private void UnsubscribeBalance()
        {
            if (!_isSubscribedToBalance || WorldPlayerInventory.Instance == null)
                return;

            WorldPlayerInventory.Instance.balance.OnValueChanged -= OnBalanceChanged;
            _isSubscribedToBalance = false;
        }

        private void OnBalanceChanged(int _)
        {
            Refresh();
        }
    }
}
