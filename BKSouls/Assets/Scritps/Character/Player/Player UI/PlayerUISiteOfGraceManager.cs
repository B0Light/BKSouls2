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
        private PlayerManager _player;

        public void OpenRestMenu(int cost, Action onConfirmed, PlayerManager player)
        {
            _restCost = cost;
            _onRestConfirmed = onConfirmed;
            _player = player;

            RefreshUI();

            restButton.onClick.RemoveAllListeners();
            restButton.onClick.AddListener(OnRestConfirmed);

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CloseMenu);

            LockPlayerMovement(true);
            OpenMenu();
        }

        public override void CloseMenu()
        {
            LockPlayerMovement(false);
            base.CloseMenu();
        }

        private void LockPlayerMovement(bool locked)
        {
            if (_player == null) return;
            _player.characterLocomotionManager.canMove = !locked;
            _player.characterLocomotionManager.canRotate = !locked;
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
