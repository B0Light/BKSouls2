using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [HideInInspector] public bool isActive = false;

        [SerializeField] private Transform mainInventoryCanvas;
        
        PlayerControls playerControls;
        [Header("Input")]
        [SerializeField] private Vector2 mouseInput;

        private InventoryHighlight _inventoryHighlight;
        private Vector2Int _oldPosition;
        private InventoryItem _itemToHighlight;

        private InventoryItem _overlapItem;

        private ItemGrid _selectedItemGrid;

        private ItemGrid _lastSelectedItemGrid;
        private Vector2Int _lastTileGridPosition;

        public ItemGrid SelectedItemGrid
        {
            get => _selectedItemGrid;
            set
            {
                if (_selectedItemGrid != value)
                {
                    _selectedItemGrid = value;
                    _inventoryHighlight.SetParent(value);
                }
            }
        }

        private InventoryItem _selectedItem;

        private InventoryItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetSelectedItemState(_selectedItem, false);
                _selectedItem = value;
                SetSelectedItemState(_selectedItem, true);

                if (_selectedItem != null)
                {
                    UpdateHighlight();
                }
                else
                {
                    _inventoryHighlight.ShowSelector(false);
                }
            }
        }

        private void SetSelectedItemState(InventoryItem item, bool isSelected)
        {
            if (item == null) return;

            item.gameObject.GetComponent<Image>().raycastTarget = !isSelected;
        }

        private void UpdateHighlight()
        {
            _inventoryHighlight.SetPosition(_selectedItemGrid, _itemToHighlight);
            _inventoryHighlight.ShowSelector(true);
            _lastSelectedItemGrid = _selectedItemGrid;
        }


        private void Awake()
        {
            _inventoryHighlight = GetComponent<InventoryHighlight>();
            isActive = false;
        }
        
        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.UI.MousePosition.performed += i => mouseInput = i.ReadValue<Vector2>();
                playerControls.UI.Click.performed += LeftMouseButtonPress;
                playerControls.UI.DoubleClick.performed += RightMouseButtonPress;
                playerControls.UI.RightClick.performed += RightMouseButtonPress;
                playerControls.UI.Rotate.performed += RotateItem;
            }

            playerControls.Enable();
        }

        private void OnDisable()
        {
            playerControls.Disable();
        }

        private void LateUpdate()
        {
            if (SelectedItem)
            {
                SelectedItem.gameObject.transform.SetParent(mainInventoryCanvas);
                SelectedItem.gameObject.transform.position = mouseInput;
            }


            if (_selectedItemGrid == null)
            {
                _inventoryHighlight.ShowHighlighter(false);
            }

            HandleHighlight();
        }

        public void RotateItem(InputAction.CallbackContext context)
        {
            if (SelectedItem == null) return;
            SelectedItem.Rotate();
        }

        private void HandleHighlight()
        {
            if (_selectedItemGrid == null)
            {
                _inventoryHighlight.ShowHighlighter(false);
                GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
                return;
            }

            Vector2Int positionOnGrid = GetTileGridPosition();
            if (SelectedItem == null)
            {
                _itemToHighlight = _selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);
                // 현재 선택된 아이템이 없는데 내가 있는 마우스 포지션에 선택할 수 있는 아이템이 있음 
                if (_itemToHighlight != null)
                {
                    _inventoryHighlight.HighlightToSelect(_itemToHighlight, _selectedItemGrid, true);

                    GUIController.Instance.inventoryGUIManager.SetItemToolTip(_itemToHighlight.itemData);
                }
                // 현재 선택된 아이템이 없는데 내가 있는 마우스 포지션이 빈칸임 
                else
                {
                    _inventoryHighlight.ShowHighlighter(false);

                    GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
                }
            }
            else
            {
                GUIController.Instance.inventoryGUIManager.CloseItemToolTip();

                _inventoryHighlight.UpdateHighlight(SelectedItem, _selectedItemGrid, positionOnGrid.x,
                    positionOnGrid.y);
            }
        }

        public void LeftMouseButtonPress(InputAction.CallbackContext context)
        {
            if (SelectedItemGrid)
            {
                _lastTileGridPosition = GetTileGridPosition();
                if (SelectedItem)
                {
                    PlaceItem(_lastTileGridPosition);
                }
                else
                {
                    PickUpItem(_lastTileGridPosition);
                }
            }
            else
            {
                if (SelectedItem)
                {
                    DiscardItem();
                }
            }
        }

        public void RightMouseButtonPress(InputAction.CallbackContext context)
        {
            if (_selectedItemGrid == null) return;
            if (SelectedItem)
            {
                ResetSelectedItem();
                return;
            }

            if (_itemToHighlight == null) return;

            GUIController.Instance.inventoryGUIManager.CloseItemToolTip();
            _lastTileGridPosition = GetTileGridPosition();

            var backpack = WorldPlayerInventory.Instance.GetBackpackInventory();
            var inventory = WorldPlayerInventory.Instance.GetInventory();
            var targetGrid = WorldPlayerInventory.Instance.curInteractItemGrid;
            var opened = WorldPlayerInventory.Instance.curOpenedInventory;

            InventoryItem pickUpItem = _selectedItemGrid.PickUpItem(_lastTileGridPosition.x, _lastTileGridPosition.y);
            if (pickUpItem == null) return;
            switch (_selectedItemGrid.itemGridType)
            {
                case ItemGrid.ItemGridType.PlayerInventory:
                case ItemGrid.ItemGridType.BackpackInventory:
                    // 1. 외부 인벤토리로 이동
                    if (opened == ItemGridType.InteractableInventory || opened == ItemGridType.ShareInventory)
                    {
                        if (!targetGrid.AddItem(pickUpItem.gameObject, false))
                        {
                            _selectedItemGrid.AddItem(pickUpItem.gameObject);
                        }

                        break;
                    }

                    // 2. 내부 사용
                    pickUpItem = _selectedItemGrid.PickUpItem(pickUpItem);
                    if (_selectedItemGrid.UseItem(pickUpItem)) break;
                    // 3. PlayerInventory <-> Backpack 간 이동


                    if (opened == ItemGridType.BackpackInventory)
                    {
                        pickUpItem = _selectedItemGrid.PickUpItem(pickUpItem);

                        if (_selectedItemGrid.itemGridType == ItemGrid.ItemGridType.BackpackInventory)
                        {
                            if (!inventory.AddItem(pickUpItem.gameObject, false))
                            {
                                _selectedItemGrid.AddItem(pickUpItem.gameObject);
                            }
                        }
                        else if (_selectedItemGrid.itemGridType == ItemGrid.ItemGridType.PlayerInventory)
                        {
                            if (!backpack.AddItem(pickUpItem.gameObject, false))
                            {
                                _selectedItemGrid.AddItem(pickUpItem.gameObject);
                            }
                        }
                    }

                    break;

                case ItemGrid.ItemGridType.InteractableInventory:
                case ItemGrid.ItemGridType.ShareInventory:
                    if (!inventory.AddItem(pickUpItem.gameObject, false) &&
                        !backpack.AddItem(pickUpItem.gameObject, false))
                    {
                        _selectedItemGrid.AddItem(pickUpItem.gameObject, false);
                    }

                    break;
                case ItemGrid.ItemGridType.EquipmentInventory:
                    if (!inventory.AddItem(pickUpItem.gameObject, false))
                    {
                        if (!backpack.AddItem(pickUpItem.gameObject, false))
                        {
                            _selectedItemGrid.AddItem(pickUpItem.gameObject, false);
                        }
                    }

                    break;

                default:
                    break;
            }
        }
        
        private Vector2Int GetTileGridPosition()
        {
            Vector2 position = mouseInput;
            if (SelectedItem != null)
            {
                position.x -= (SelectedItem.Width - 1) * ItemGrid.TileSizeWidth / 2;
                position.y += (SelectedItem.Height - 1) * ItemGrid.TileSizeHeight / 2;
            }

            return _selectedItemGrid.GetTileGridPosition(position);
        }

        private void PickUpItem(Vector2Int tileGridPosition)
        {
            SelectedItem = _selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);
        }

        private void PlaceItem(Vector2Int tileGridPosition)
        {
            if (_selectedItemGrid.PlaceItem(SelectedItem, tileGridPosition.x, tileGridPosition.y, false))
                SelectedItem = null;
        }

        public Transform GetItemOnPointerTransform()
        {
            if (_selectedItemGrid == null) return null;
            Vector2Int positionOnGrid = GetTileGridPosition();
            InventoryItem inventoryItem = _selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

            return inventoryItem ? inventoryItem.transform : null;
        }

        public void ResetSelectedItem()
        {
            if (!SelectedItem) return;

            if (_lastSelectedItemGrid.PlaceItem(SelectedItem, SelectedItem.onGridPositionX,
                    SelectedItem.onGridPositionY, false))
            {
                //Destroy(SelectedItem.gameObject);
                _lastSelectedItemGrid = null;
                SelectedItem = null;
            }
            else
            {
                Debug.LogWarning("Something wrong");
            }
        }

        private void DiscardItem()
        {
            if (!SelectedItem) return;

            int itemId = SelectedItem.itemData.itemID;
            
            // - Host/Server면 서버에서 바로 처리 가능
            // - Client면 ServerRpc로 요청
            RequestDropItem(itemId);

            // 손에 들고 있는 UI 아이템 제거 (로컬 UI)
            Destroy(SelectedItem.gameObject);
            _lastSelectedItemGrid = null;
            SelectedItem = null;
        }

        public static void RequestDropItem(int itemId)
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening)
            {
                // 네트워크 안 쓰는 순수 싱글이면 기존 로컬 드랍 로직으로 처리하거나 무시
                Debug.LogWarning("[Inventory] Network not running, cannot drop network item.");
                return;
            }

            // 로컬 플레이어 오브젝트에서 Dropper 찾기
            var playerObj = nm.LocalClient?.PlayerObject;
            if (playerObj == null)
            {
                Debug.LogWarning("[Inventory] Local player object not ready.");
                return;
            }

            var dropper = playerObj.GetComponent<PlayerItemDropper>();
            if (dropper == null)
            {
                Debug.LogError("[Inventory] PlayerItemDropper not found on local player.");
                return;
            }

            // 서버는 직접 호출해도 되나 일관성을 위해 rpc 호출
            dropper.RequestDropItemServerRpc(itemId);
        }
    }
}