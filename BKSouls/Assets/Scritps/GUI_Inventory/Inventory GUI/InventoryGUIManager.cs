using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class InventoryGUIManager : GUIComponent
    {
        [Header("Inventory System")] 
        public InventoryController inventoryController;
        [SerializeField] private GameObject itemTooltip;

        [Header("Player Equipment Inventory")] 
        public ItemGrid_Equipment playerRightWeapon;
        public ItemGrid_Equipment playerLeftWeapon;
        public ItemGrid_Equipment playerRightSubWeapon;
        public ItemGrid_Equipment playerLeftSubWeapon;
        public ItemGrid_Equipment playerHelmet;
        public ItemGrid_Equipment playerArmor;
        public ItemGrid_Equipment playerGauntlet;
        public ItemGrid_Equipment playerLeggings;

        [Header("Weapon Grid Canvas Group")] 
        [SerializeField] private CanvasGroup rightMainWeaponCanvasGroup;
        [SerializeField] private CanvasGroup rightSubWeaponCanvasGroup;
        [SerializeField] private CanvasGroup leftMainWeaponCanvasGroup;
        [SerializeField] private CanvasGroup leftSubWeaponCanvasGroup;

        [SerializeField] private Button activeRightMain;
        [SerializeField] private Button activeRightSub;
        [SerializeField] private Button activeLeftMain;
        [SerializeField] private Button activeLeftSub;

        [Header("Player Inventory")] 
        public ItemGrid playerInventoryItemGrid;
        public ItemGrid backpackItemGrid;
        private bool _hasBackpack = false;

        [Header("Safe Inventory")] 
        public ItemGrid safeInventoryItemGrid;
        [SerializeField] private CanvasGroup safeInventoryCanvasGroup;

        [Header("Interactable Inventory")] 
        [SerializeField] private ItemGrid_Interactable interactionInventoryItemGrid;
        [SerializeField] private CanvasGroup interactionInventoryCanvasGroup;

        [Header("Share Inventory")] 
        public ItemGrid shareInventoryItemGrid;
        [SerializeField] private CanvasGroup shareInventoryObject;

        [Header("Forge Inventory")] 
        [SerializeField] private ForgeGUIManager forgeGUIManager;

        [SerializeField] private CanvasGroup forgeInventoryCanvasGroup;

        [Header("Crusher Inventory")] 
        [SerializeField] private ShredderHUDManager shredderHUDManager;

        [SerializeField] private CanvasGroup shredderCanvasGroup;

        private Interactable _interactableObject;

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private void Start()
        {
            _isOpen = false;
            CloseInteractionInventory();
            CloseItemToolTip();

            InitWeaponGridButton();
        }

        private void InitWeaponGridButton()
        {
            activeRightMain.onClick.AddListener(OpenRightMainWeaponGrid);
            activeRightSub.onClick.AddListener(OpenRightSubWeaponGrid);
            activeLeftMain.onClick.AddListener(OpenLeftMainWeaponGrid);
            activeLeftSub.onClick.AddListener(OpenLeftSubWeaponGrid);
            
            OpenRightMainWeaponGrid();
            OpenLeftMainWeaponGrid();
        }

        public override void OpenGUI()
        {
            if (_isOpen)
                return;

            base.OpenGUI();
            _isOpen = true;
            inventoryController.isActive = _isOpen;

            WorldPlayerInventory.Instance.curOpenedInventory =
                _hasBackpack ? ItemGridType.BackpackInventory : ItemGridType.PlayerInventory;
            WorldPlayerInventory.Instance.curInteractItemGrid =
                _hasBackpack ? backpackItemGrid : playerInventoryItemGrid;
        }

        public override void CloseGUI()
        {
            if (!_isOpen)
                return;

            base.CloseGUI();
            _isOpen = false;
            inventoryController.isActive = _isOpen;

            ResetSelectItem();
            CloseInteractionInventory();

            GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
            WorldPlayerInventory.Instance.curOpenedInventory = ItemGridType.None;
            WorldPlayerInventory.Instance.curInteractItemGrid = null;
        }

        private void CloseInteractionInventory()
        {
            // if (_interactableObject) _interactableObject.ResetInteraction();
            _interactableObject = null;
            ToggleInteractionInventory(false);
            ToggleShareInventory(false);
            ToggleForge(false);
            ToggleShredder(false);
        }

        public void ActiveSafe(bool isActive)
        {
            safeInventoryCanvasGroup.alpha = isActive ? 1 : 0;
            safeInventoryCanvasGroup.interactable = isActive;
            safeInventoryCanvasGroup.blocksRaycasts = isActive;
        }

        public void OpenInteractionInventory(bool isShareInventory, int width, int height, List<int> itemIdList, Interactable interactable)
        {
            _interactableObject = interactable;

            ToggleInteractionInventory(!isShareInventory);
            ToggleShareInventory(isShareInventory);

            var targetGrid = isShareInventory ? shareInventoryItemGrid : interactionInventoryItemGrid;

            Debug.LogWarning($"CUR BOX SIZE [{width}, {height}]");
            targetGrid.ResetItemGrid();
            targetGrid.SetGrid(width, height, itemIdList);

            WorldPlayerInventory.Instance.curOpenedInventory =
                isShareInventory ? ItemGridType.ShareInventory : ItemGridType.InteractableInventory;

            WorldPlayerInventory.Instance.curInteractItemGrid = targetGrid;
        }

        public void OpenInteractionShredder(int width, int height, List<int> itemIdList, Interactable interactable)
        {
            _interactableObject = interactable;

            ToggleShredder(true);

            shredderHUDManager.Init(width, height, itemIdList);

            WorldPlayerInventory.Instance.curOpenedInventory = ItemGridType.InteractableInventory;

            WorldPlayerInventory.Instance.curInteractItemGrid = shredderHUDManager.GetItemGrid;
        }

        private void ResetSelectItem()
        {
            inventoryController.ResetSelectedItem();
        }

        public void OpenForge(Interactable interactable)
        {
            _interactableObject = interactable;

            ToggleForge(true);

            forgeGUIManager.InitForge();

            WorldPlayerInventory.Instance.curOpenedInventory = ItemGridType.InteractableInventory;

            WorldPlayerInventory.Instance.curInteractItemGrid = forgeGUIManager.GetItemGrid;
        }

        private void OpenRightMainWeaponGrid()
        {
            rightMainWeaponCanvasGroup.alpha = 1;
            rightMainWeaponCanvasGroup.interactable = true;
            rightMainWeaponCanvasGroup.blocksRaycasts = true;
            
            rightSubWeaponCanvasGroup.alpha = 0;
            rightSubWeaponCanvasGroup.interactable = false;
            rightSubWeaponCanvasGroup.blocksRaycasts = false;

            activeRightMain.interactable = false;
            activeRightSub.interactable = true;
        }
        
        private void OpenRightSubWeaponGrid()
        {
            rightMainWeaponCanvasGroup.alpha = 0;
            rightMainWeaponCanvasGroup.interactable = false;
            rightMainWeaponCanvasGroup.blocksRaycasts = false;
            
            rightSubWeaponCanvasGroup.alpha = 1;
            rightSubWeaponCanvasGroup.interactable = true;
            rightSubWeaponCanvasGroup.blocksRaycasts = true;
            
            activeRightMain.interactable = true;
            activeRightSub.interactable = false;
        }
        
        private void OpenLeftMainWeaponGrid()
        {
            leftMainWeaponCanvasGroup.alpha = 1;
            leftMainWeaponCanvasGroup.interactable = true;
            leftMainWeaponCanvasGroup.blocksRaycasts = true;
            
            leftSubWeaponCanvasGroup.alpha = 0;
            leftSubWeaponCanvasGroup.interactable = false;
            leftSubWeaponCanvasGroup.blocksRaycasts = false;
            
            activeLeftMain.interactable = false;
            activeLeftSub.interactable = true;
        }
        
        private void OpenLeftSubWeaponGrid()
        {
            leftMainWeaponCanvasGroup.alpha = 0;
            leftMainWeaponCanvasGroup.interactable = false;
            leftMainWeaponCanvasGroup.blocksRaycasts = false;
            
            leftSubWeaponCanvasGroup.alpha = 1;
            leftSubWeaponCanvasGroup.interactable = true;
            leftSubWeaponCanvasGroup.blocksRaycasts = true;
            
            activeLeftMain.interactable = true;
            activeLeftSub.interactable = false;
        }

        public void ToggleBackpackInventory(bool isActive)
        {
            backpackItemGrid.gameObject.SetActive(isActive);

            _hasBackpack = isActive;
        }

        private void ToggleInteractionInventory(bool isActive)
        {
            interactionInventoryCanvasGroup.alpha = isActive ? 1 : 0;
            interactionInventoryCanvasGroup.interactable = isActive;
            interactionInventoryCanvasGroup.blocksRaycasts = isActive;
        }

        private void ToggleShareInventory(bool isActive)
        {
            shareInventoryObject.alpha = isActive ? 1 : 0;
            shareInventoryObject.interactable = isActive;
            shareInventoryObject.blocksRaycasts = isActive;
        }

        private void ToggleForge(bool isActive)
        {
            forgeInventoryCanvasGroup.alpha = isActive ? 1 : 0;
            forgeInventoryCanvasGroup.interactable = isActive;
            forgeInventoryCanvasGroup.blocksRaycasts = isActive;
        }

        private void ToggleShredder(bool isActive)
        {
            shredderCanvasGroup.alpha = isActive ? 1 : 0;
            shredderCanvasGroup.interactable = isActive;
            shredderCanvasGroup.blocksRaycasts = isActive;
        }

        /* TOOL TIP */
        public void SetItemToolTip(GridItem itemInfo)
        {
            CanvasGroup cg = itemTooltip.GetComponent<CanvasGroup>();
            cg.alpha = 1;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            HUD_SelectedItemInfo toolTip = itemTooltip.GetComponent<HUD_SelectedItemInfo>();
            if (!toolTip) return;

            toolTip.Init(itemInfo);
            Transform refTr = inventoryController.GetItemOnPointerTransform();
            if (refTr)
            {
                RectTransform tooltipRect = itemTooltip.GetComponent<RectTransform>();

                // 현재 선택된 아이템의 좌표
                Vector2 tooltipPosition = refTr.transform.position;

                // 화면의 절반 좌표 계산
                float screenWidthHalf = Screen.width / 2f;
                float screenHeightHalf = Screen.height / 2f;

                // 기본 툴팁 위치 계산 (아이템의 크기 반영)
                tooltipPosition.x += itemInfo.width * ItemGrid.TileSizeWidth / 2;
                tooltipPosition.y += itemInfo.height * ItemGrid.TileSizeHeight / 2;

                // 툴팁 위치 조정
                if (tooltipPosition.x > screenWidthHalf)
                {
                    // 아이템이 화면 오른쪽에 있으면 툴팁을 왼쪽으로 이동
                    tooltipPosition.x -= itemInfo.width * ItemGrid.TileSizeWidth + tooltipRect.rect.width;
                }

                if (tooltipPosition.y < screenHeightHalf)
                {
                    // 아이템이 화면 아래쪽에 있으면 툴팁을 위로 이동
                    tooltipPosition.y += (tooltipRect.rect.height - itemInfo.height * ItemGrid.TileSizeHeight) *
                                         Mathf.Clamp01((Screen.height - tooltipPosition.y) / Screen.height);
                }

                tooltipRect.position = tooltipPosition;

                return;
            }

            CloseItemToolTip();
        }

        public void CloseItemToolTip()
        {
            CanvasGroup cg = itemTooltip.GetComponent<CanvasGroup>();
            cg.alpha = 0;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }
}

public enum ItemGridType
{
    PlayerInventory,
    InteractableInventory,
    EquipmentInventory,
    ShareInventory,
    BackpackInventory,
    None,
}