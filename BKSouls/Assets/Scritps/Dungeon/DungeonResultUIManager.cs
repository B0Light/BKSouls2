using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using BK.Inventory;

namespace BK
{
    public class DungeonResultUIManager : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI roomsClearedText;
        [SerializeField] private TextMeshProUGUI shelterCoinText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI runesSpentText;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        [Header("Equipment")]
        [SerializeField] private Image[] equipmentIconSlots;
        [SerializeField] private TextMeshProUGUI[] equipmentNameTexts;
        [SerializeField] private Sprite emptyEquipmentIcon;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;

        private Action onConfirmed;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            Close();
        }

        public void Open(DungeonResultData resultData, Action confirmAction)
        {
            onConfirmed = confirmAction;
            IsOpen = true;

            if (titleText != null)
                titleText.text = resultData.isClear ? "Dungeon Cleared" : "Dungeon Failed";

            if (descriptionText != null)
                descriptionText.text = resultData.isClear
                    ? "The run is complete. Return to the shelter with your rewards."
                    : "The run has ended. Return to the shelter and recover.";

            if (roomsClearedText != null)
                roomsClearedText.text = $"Rooms Cleared : {resultData.roomsCleared}";

            if (shelterCoinText != null)
                shelterCoinText.text = $"Shelter Coin : {resultData.shelterCoinGain}";

            if (levelText != null)
                levelText.text = resultData.isClear ? $"Level : {resultData.playerLevel}" : string.Empty;

            if (runesSpentText != null)
                runesSpentText.text = resultData.isClear ? $"Runes Spent : {resultData.runesSpent}" : string.Empty;

            if (confirmButtonText != null)
                confirmButtonText.text = "Return";

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(Confirm);
                confirmButton.Select();
            }

            SetVisible(true);
            PopulateEquipment(GUIController.Instance != null ? GUIController.Instance.localPlayer : null);
            GUIController.ShowCursor();
        }

        private void PopulateEquipment(PlayerManager player)
        {
            Item[] equipmentItems = GetEquipmentItems(player);

            int slotCount = equipmentIconSlots != null ? equipmentIconSlots.Length : 0;
            for (int i = 0; i < slotCount; i++)
            {
                Item item = i < equipmentItems.Length ? equipmentItems[i] : null;
                SetEquipmentSlot(i, item);
            }
        }

        private static Item[] GetEquipmentItems(PlayerManager player)
        {
            if (player == null || player.playerInventoryManager == null)
                return Array.Empty<Item>();

            PlayerInventoryManager inv = player.playerInventoryManager;
            return new Item[]
            {
                inv.currentRightHandWeapon,
                inv.currentLeftHandWeapon,
                inv.currentSpell,
                inv.currentQuickSlotItem,
                inv.headEquipment,
                inv.bodyEquipment,
                inv.handEquipment,
                inv.legEquipment,
                inv.mainProjectile,
                inv.secondaryProjectile
            };
        }

        private void SetEquipmentSlot(int index, Item item)
        {
            if (equipmentIconSlots != null && index < equipmentIconSlots.Length && equipmentIconSlots[index] != null)
            {
                equipmentIconSlots[index].sprite = item != null && item.itemIcon != null
                    ? item.itemIcon
                    : emptyEquipmentIcon;
                equipmentIconSlots[index].enabled = equipmentIconSlots[index].sprite != null;
            }

            if (equipmentNameTexts != null && index < equipmentNameTexts.Length && equipmentNameTexts[index] != null)
                equipmentNameTexts[index].text = item != null ? item.itemName : string.Empty;
        }

        public void Confirm()
        {
            if (!IsOpen)
                return;

            Action confirmAction = onConfirmed;
            Close();
            confirmAction?.Invoke();
        }

        public void Close()
        {
            onConfirmed = null;
            IsOpen = false;
            SetVisible(false);

            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = isVisible ? 1 : 0;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }

    [Serializable]
    public struct DungeonResultData : INetworkSerializable
    {
        public bool isClear;
        public int roomsCleared;
        public int shelterCoinGain;
        public int playerLevel;
        public int runesSpent;

        public DungeonResultData(bool isClear, int roomsCleared, int shelterCoinGain, int playerLevel, int runesSpent)
        {
            this.isClear = isClear;
            this.roomsCleared = roomsCleared;
            this.shelterCoinGain = shelterCoinGain;
            this.playerLevel = playerLevel;
            this.runesSpent = runesSpent;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref isClear);
            serializer.SerializeValue(ref roomsCleared);
            serializer.SerializeValue(ref shelterCoinGain);
            serializer.SerializeValue(ref playerLevel);
            serializer.SerializeValue(ref runesSpent);
        }
    }
}
