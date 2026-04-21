using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK
{
    public class PlayerUISiteOfGraceManager : PlayerUIMenu
    {
        [Header("Rest UI")]
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private Button restButton;
        [SerializeField] private Button cancelButton;

        private int _restCost;
        private Action _onRestConfirmed;

        public void OpenRestMenu(int cost, Action onConfirmed)
        {
            _restCost = cost;
            _onRestConfirmed = onConfirmed;

            RefreshUI();

            restButton.onClick.RemoveAllListeners();
            restButton.onClick.AddListener(OnRestConfirmed);

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseMenu);

            OpenMenu();
        }

        private void RefreshUI()
        {
            int balance = BK.Inventory.WorldPlayerInventory.Instance.balance.Value;
            costText.text = $"Cost : {_restCost}";
            balanceText.text = $"Runes : {balance}";
            restButton.interactable = balance >= _restCost;
        }

        private void OnRestConfirmed()
        {
            if (BK.Inventory.WorldPlayerInventory.Instance.balance.Value < _restCost)
                return;

            BK.Inventory.WorldPlayerInventory.Instance.balance.Value -= _restCost;
            _onRestConfirmed?.Invoke();
            CloseMenu();
        }

        public void OpenLevelUpMenu()
        {
            CloseMenu();
            GUIController.Instance.playerUILevelUpManager.OpenMenu();
        }
    }
}
