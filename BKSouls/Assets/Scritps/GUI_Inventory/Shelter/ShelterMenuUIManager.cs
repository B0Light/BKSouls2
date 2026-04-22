using System.Collections.Generic;
using BK.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK
{
    public class ShelterMenuUIManager : MonoBehaviour
    {
        [Header("Upgrade Config")]
        [SerializeField] private List<int> upgradeCostPerLevel = new List<int> { 100, 250, 600, 1500, 3500 };
        [SerializeField] private List<int> visitorCapacityPerLevel = new List<int> { 3, 5, 7, 10, 15, 20 };

        [Header("Current Info")]
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentVisitorText;
        [SerializeField] private TextMeshProUGUI currentRarityText;

        [Header("Next Info")]
        [SerializeField] private TextMeshProUGUI nextLevelText;
        [SerializeField] private TextMeshProUGUI nextVisitorText;
        [SerializeField] private TextMeshProUGUI nextRarityText;

        [Header("Upgrade Cost")]
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        [Header("Unlocked Buildings Preview")]
        [SerializeField] private Transform buildingIconContainer;
        [SerializeField] private GameObject buildingIconPrefab;
        [SerializeField] private HUDGridBuildToSelectInfo buildingInfoPopup;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button upgradeButton;

        private CanvasGroup _canvasGroup;
        private static readonly int MaxShelterLevel = (int)ItemTier.Mythic;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Open()
        {
            WorldPlayerInventory.Instance.balance.OnValueChanged += OnBalanceChanged;

            RefreshUI();
            SetVisible(true);

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);

            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(TryUpgrade);
        }

        public void Close()
        {
            WorldPlayerInventory.Instance.balance.OnValueChanged -= OnBalanceChanged;
            SetVisible(false);
        }

        private void OnBalanceChanged(int value)
        {
            RefreshUpgradeButton();
        }

        private void RefreshUI()
        {
            int currentLevel = WorldSaveGameManager.Instance.currentCharacterData.shelterLevel;
            int nextLevel = currentLevel + 1;
            bool canUpgrade = currentLevel < MaxShelterLevel;

            if (currentLevelText != null)
                currentLevelText.text = currentLevel.ToString();
            if (currentVisitorText != null)
                currentVisitorText.text = GetVisitorCapacity(currentLevel).ToString();
            if (currentRarityText != null)
                currentRarityText.text = ((ItemTier)currentLevel).ToString();

            if (canUpgrade)
            {
                if (nextLevelText != null)
                    nextLevelText.text = nextLevel.ToString();
                if (nextVisitorText != null)
                    nextVisitorText.text = GetVisitorCapacity(nextLevel).ToString();
                if (nextRarityText != null)
                    nextRarityText.text = ((ItemTier)nextLevel).ToString();
                if (upgradeCostText != null)
                    upgradeCostText.text = GetUpgradeCost(currentLevel).ToString();

                PopulateBuildingIcons((ItemTier)nextLevel);
            }
            else
            {
                if (nextLevelText != null) nextLevelText.text = "-";
                if (nextVisitorText != null) nextVisitorText.text = "-";
                if (nextRarityText != null) nextRarityText.text = "-";
                if (upgradeCostText != null) upgradeCostText.text = "MAX";
                ClearBuildingIcons();
            }

            RefreshUpgradeButton();
        }

        private void RefreshUpgradeButton()
        {
            if (upgradeButton == null) return;
            int currentLevel = WorldSaveGameManager.Instance.currentCharacterData.shelterLevel;
            bool canUpgrade = currentLevel < MaxShelterLevel;
            int cost = GetUpgradeCost(currentLevel);
            int balance = WorldPlayerInventory.Instance.balance.Value;
            upgradeButton.interactable = canUpgrade && balance >= cost;
        }

        private void TryUpgrade()
        {
            int currentLevel = WorldSaveGameManager.Instance.currentCharacterData.shelterLevel;
            if (currentLevel >= MaxShelterLevel) return;

            int cost = GetUpgradeCost(currentLevel);
            if (!WorldPlayerInventory.Instance.TrySpend(cost)) return;

            WorldSaveGameManager.Instance.currentCharacterData.shelterLevel++;

            CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager;
            if (categoryBuildHUDManager != null)
                categoryBuildHUDManager.UpdateAvailableBuildings();

            WorldSaveGameManager.Instance.SaveGame();
            RefreshUI();
        }

        private void PopulateBuildingIcons(ItemTier tier)
        {
            ClearBuildingIcons();
            if (buildingIconPrefab == null || buildingIconContainer == null) return;

            var buildings = WorldDatabase_Build.Instance.GetBuildingsByTierReadOnly(tier);
            foreach (var building in buildings)
            {
                GameObject iconObj = Instantiate(buildingIconPrefab, buildingIconContainer);

                Image iconImage = iconObj.GetComponent<Image>();
                if (iconImage != null && building.itemIcon != null)
                    iconImage.sprite = building.itemIcon;

                TextMeshProUGUI nameText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = building.itemName;

                Button btn = iconObj.GetComponent<Button>();
                if (btn != null && buildingInfoPopup != null)
                {
                    BuildObjData captured = building;
                    btn.onClick.AddListener(() => buildingInfoPopup.Show(captured));
                }
            }
        }

        private void ClearBuildingIcons()
        {
            if (buildingIconContainer == null) return;
            foreach (Transform child in buildingIconContainer)
                Destroy(child.gameObject);
        }

        private int GetVisitorCapacity(int level)
        {
            if (level < visitorCapacityPerLevel.Count)
                return visitorCapacityPerLevel[level];
            return visitorCapacityPerLevel.Count > 0
                ? visitorCapacityPerLevel[visitorCapacityPerLevel.Count - 1]
                : 0;
        }

        private int GetUpgradeCost(int currentLevel)
        {
            if (currentLevel < upgradeCostPerLevel.Count)
                return upgradeCostPerLevel[currentLevel];
            return upgradeCostPerLevel.Count > 0
                ? Mathf.RoundToInt(upgradeCostPerLevel[upgradeCostPerLevel.Count - 1] * Mathf.Pow(2f, currentLevel - upgradeCostPerLevel.Count + 1))
                : 0;
        }

        private void SetVisible(bool isActive)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = isActive ? 1f : 0f;
            _canvasGroup.interactable = isActive;
            _canvasGroup.blocksRaycasts = isActive;
        }
    }
}
