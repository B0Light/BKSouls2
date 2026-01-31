using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace BK.Inventory
{
    public class ItemGrid : MonoBehaviour
    {
        public enum ItemGridType
        {
            PlayerInventory,
            InteractableInventory,
            EquipmentInventory,
            ShareInventory,
            BackpackInventory,
            None,
        }

        public ItemGridType itemGridType;
        public const float TileSizeWidth = 64;
        public const float TileSizeHeight = 64;

        private InventoryItem[,] _inventoryItemSlot;

        private RectTransform _rectTransform;
        protected Vector2 GUISize;

        private Vector2Int gridSize;

        private Vector2 _positionOnTheGrid = new Vector2(); //Inventory상의 기준점 설정
        private Vector2Int _tileGridPosition = new Vector2Int();

        private List<int> _interactObjectList;

        private Multimap<ItemInfo, Vector2> _itemPosDict;
        // 문 상호작용 등 인벤토리에서 직접 아이템을 제거하는 경우  

        protected SerializedDictionary<int, int> itemIdToCntDict = new SerializedDictionary<int, int>();
        // 현재 그리드에 포함된 아이템 딕셔너리 id - cnt 
        // 해당 아이템 그리드에 저장된 아이템을 세이브 파일에 저장하기 위한 데이터 (id, cnt)

        public Variable<float> itemGridWeight = new Variable<float>(0);
        public Variable<int> totalItemValue = new Variable<int>(0);

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>(); //UI의 기준점 위치
        }

        public void UpdateItemGridSize(Vector2Int newGridSize)
        {
            List<int> itemList = new List<int>();
            foreach (var items in itemIdToCntDict)
            {
                for (int i = 0; i < items.Value; i++)
                    itemList.Add(items.Key);
            }

            ResetItemGrid();
            SetGrid(newGridSize.x, newGridSize.y, itemList);
        }

        public virtual void SetGrid(int width, int height, List<int> setItemList)
        {
            Init(width, height);
            if (setItemList == null || setItemList.Count == 0) return;
            _interactObjectList = setItemList;
            PlaceItemsAuto(_interactObjectList);
        }

        //Set Inventory size
        protected virtual void Init(int width, int height)
        {
            //Debug.LogWarning("[GIRD SIZE] : ("+width+", "+ height +")");
            gridSize.x = width;
            gridSize.y = height;
            _inventoryItemSlot = new InventoryItem[width, height];
            _itemPosDict = new Multimap<ItemInfo, Vector2>();
            GUISize = new Vector2(width * TileSizeWidth, height * TileSizeHeight);
            _rectTransform.sizeDelta = GUISize;
        }

        public Vector2Int GetCurItemGridSize() => gridSize;

        /*
         * isLoad는 스폰과 동시에 아이템을 초기화 하는것과 같이
         * 이미 아이템 데이터는 가지고 있으나 ui상 표시하기 위해 사용하는 경우
         *
         * interactableBox에 아이템을 넣는 경우
         * 1. 스폰되면서 아이템이 들어감
         * 2. 플레이어가 아이템을 집어넣음
         *
         * 1과 2의 과정은 사실상 동일하여 같은 함수를 사용하지만
         * 2는 아이템을 넣으면서 박스에 넣은 아이템의 id를 추가하지만
         * 1은 이미 id가 추가되어 있는 상황임
         */
        protected virtual void PlaceItemsAuto(List<int> setItemList, bool isLoad = true)
        {
            if (setItemList == null) return;
            foreach (var itemCode in setItemList)
            {
                if (AddItemById(itemCode, isLoad: isLoad)) continue;


                /*
                 -----------item Drop 방식을 변경----------
                // 새로 아이템 추가에 실패 -> 아이템 드롭
                // 플레이어가 슬롯 특전 반환하며 아이템 슬롯이 줄어들 경우 
                if (!IsBoxGrid()) // 파밍 레벨에서 현재 아이템 박스보다 큰 아이템이 들어있는 경우에 대해서는 드롭하지 않음 
                {
                    Vector3 spawnPos = GameManager.Instance.GetPlayer().transform.position + new Vector3(0, 1, 0.5f);
                    GameObject item = Instantiate(WorldDatabase_Item.Instance.emptyInteractItemPrefab, spawnPos,
                        Quaternion.identity);
                    InteractableItem interactableItem = item.GetComponentInChildren<InteractableItem>();
                    interactableItem.SetItemCode(itemCode);
                }
                */
            }
        }

        private bool IsBoxGrid()
        {
            return itemGridType == ItemGridType.InteractableInventory;
        }


        public bool AddItemById(int itemCode, int count = 1, bool isLoad = true)
        {
            bool allAdded = true;

            for (int i = 0; i < count; i++)
            {
                GameObject spawnItem = Instantiate(WorldShopManager.Instance.inventoryItemRef);
                InventoryItem inventoryItem = spawnItem.GetComponent<InventoryItem>();
                inventoryItem.itemInfoData = WorldDatabase_Item.Instance.GetItemByID(itemCode);

                bool added = AddItem(spawnItem, isLoad);
                if (!added)
                {
                    allAdded = false;
                    Destroy(spawnItem); // 실패한 아이템은 제거
                    break; // 또는 continue; // 실패한 이후에도 계속 시도하려면 continue 사용
                }
            }

            return allAdded;
        }

        public int AddItemById_FailCount(int itemCode, int count = 1, bool isLoad = true)
        {
            int failCount = 0; // 실패 개수 카운트

            for (int i = 0; i < count; i++)
            {
                GameObject spawnItem = Instantiate(WorldShopManager.Instance.inventoryItemRef);
                InventoryItem inventoryItem = spawnItem.GetComponent<InventoryItem>();
                inventoryItem.itemInfoData = WorldDatabase_Item.Instance.GetItemByID(itemCode);

                bool added = AddItem(spawnItem, isLoad);
                if (!added)
                {
                    failCount++; // 실패 개수 증가
                    Destroy(spawnItem); // 실패한 아이템은 제거
                }
            }

            return failCount; // 실패한 개수 반환
        }

        public bool AddItem(GameObject item, bool isLoad = true)
        {
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.Set();

            if (inventoryItem.rotated) inventoryItem.Rotate();
            if (TryPlaceItem(inventoryItem, isLoad))
                return true;

            // 아이템 회전 후 재시도
            inventoryItem.Rotate();
            return TryPlaceItem(inventoryItem, isLoad);
        }

        private bool TryPlaceItem(InventoryItem item, bool isLoad)
        {
            Vector2Int? posOnGrid = FindSpaceForObject(item);
            if (posOnGrid != null)
            {
                return PlaceItem(item, posOnGrid.Value.x, posOnGrid.Value.y, isLoad);
            }

            return false;
        }

        public int GetItemCountById(int id)
        {
            return itemIdToCntDict.GetValueOrDefault(id);
        }

        public bool RemoveItems(List<int> ids)
        {
            if (!CheckItemInInventory(ids)) return false;
            foreach (var id in ids)
            {
                if (!RemoveItem(id))
                {
                    Debug.LogError("[ItemGrid] : Error - remove item");
                }
            }

            return true;
        }

        private bool CheckItemInInventory(List<int> ids)
        {
            Dictionary<int, int> chkItemDic = CountOccurrencesToDictionary(ids);

            foreach (var itemInfo in chkItemDic)
            {
                if (itemIdToCntDict[itemInfo.Key] < itemInfo.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private bool RemoveItem(int id)
        {
            ItemInfo itemInfo = FindItemById(id);
            if (itemInfo)
            {
                _itemPosDict.TryGetValuesByKey(itemInfo, out var itemPositions);

                Vector2 itemPos = itemPositions.FirstOrDefault();
                InventoryItem inventoryItem = _inventoryItemSlot[(int)itemPos.x, (int)itemPos.y];
                return RemoveItem(inventoryItem);
            }

            return false;
        }

        protected bool RemoveItem(InventoryItem inventoryItem)
        {
            if (!inventoryItem) return false;

            _itemPosDict.RemoveByKey(inventoryItem.itemInfoData);
            CleanGridReference(inventoryItem);
            itemGridWeight.Value -= inventoryItem.itemInfoData.weight;
            totalItemValue.Value -= inventoryItem.itemInfoData.purchaseCost;
            Destroy(inventoryItem.gameObject);
            return true;
        }

        // 제거한 아이템 갯수 반환 
        public int RemoveItemsById(int itemId, int count)
        {
            int removedCount = 0;

            while (removedCount < count)
            {
                ItemInfo itemInfo = FindItemById(itemId);
                if (itemInfo == null) break;

                if (_itemPosDict.TryGetValuesByKey(itemInfo, out var itemPositions) &&
                    itemPositions is ICollection<Vector2> collection && collection.Count > 0)
                {
                    Vector2 itemPos = itemPositions.First();
                    InventoryItem inventoryItem = _inventoryItemSlot[(int)itemPos.x, (int)itemPos.y];

                    if (RemoveItem(inventoryItem)) removedCount++;
                    else break; // 제거 실패 시 루프 종료
                }
                else
                {
                    break; // 해당 아이템의 위치 정보가 없거나 비어 있음
                }
            }

            return removedCount;
        }

        [CanBeNull]
        private ItemInfo FindItemById(int id)
        {
            foreach (ItemInfo item in _itemPosDict.GetAllKeys())
            {
                if (item.itemCode == id)
                {
                    return item;
                }
            }

            return null;
        }


        //카메라 모드 오버레이일때 화면의 고정된 위치에 생성
        public Vector2Int GetTileGridPosition(Vector2 mousePosition)
        {
            _positionOnTheGrid.x = mousePosition.x -
                                   (_rectTransform.position.x - _rectTransform.rect.width * _rectTransform.pivot.x);
            _positionOnTheGrid.y = (_rectTransform.position.y + _rectTransform.rect.height * _rectTransform.pivot.y) -
                                   mousePosition.y;

            _tileGridPosition.x = (int)(_positionOnTheGrid.x / TileSizeWidth);
            _tileGridPosition.y = (int)(_positionOnTheGrid.y / TileSizeHeight);
            return _tileGridPosition;
        }

        public virtual bool CheckPlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            // 인벤의 크기 외부에 있으면 위치 불가 
            if (BoundaryCheck(posX, posY, inventoryItem.Width, inventoryItem.Height) == false)
            {
                return false;
            }

            if (OverlapCheck(posX, posY, inventoryItem.Width, inventoryItem.Height) == false)
            {
                return false;
            }

            return true;
        }

        public virtual bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad)
        {
            if (!CheckPlaceItem(inventoryItem, posX, posY))
            {
                return false;
            }

            inventoryItem.previousItemGrid = this;

            RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
            rectTransform.SetParent(_rectTransform);

            for (int x = 0; x < inventoryItem.Width; x++)
            {
                for (int y = 0; y < inventoryItem.Height; y++)
                {
                    _inventoryItemSlot[posX + x, posY + y] = inventoryItem;
                }
            }

            inventoryItem.onGridPositionX = posX;
            inventoryItem.onGridPositionY = posY;

            Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);

            rectTransform.localPosition = position;
            rectTransform.localScale = Vector3.one;

            _itemPosDict.Add(inventoryItem.itemInfoData, new Vector2(posX, posY));
            AddValue(inventoryItem.itemInfoData.itemCode);
            itemGridWeight.Value += inventoryItem.itemInfoData.weight;
            totalItemValue.Value += inventoryItem.itemInfoData.purchaseCost;
            return true;
        }

        public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
        {
            Vector2 position = new Vector2();
            position.x = posX * TileSizeWidth + TileSizeWidth * inventoryItem.Width / 2 -
                         _rectTransform.rect.width * _rectTransform.pivot.x;
            position.y = -(posY * TileSizeHeight + TileSizeHeight * inventoryItem.Height / 2) +
                         _rectTransform.rect.height * _rectTransform.pivot.y;
            return position;
        }

        private bool OverlapCheck(int posX, int posY, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (_inventoryItemSlot[posX + x, posY + y] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // 상점을 사용하는 경우 아이템 픽업이 아닌 단순 선택만 필요
        public InventoryItem SelectItemOnGridPos(int x, int y)
        {
            InventoryItem pickUpItem = _inventoryItemSlot[x, y];
            return pickUpItem;
        }

        public virtual InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem pickUpItem = _inventoryItemSlot[x, y];

            if (pickUpItem == null) return null;

            CleanGridReference(pickUpItem);
            _itemPosDict.RemoveByValue(new Vector2(x, y));

            itemGridWeight.Value -= pickUpItem.itemInfoData.weight;
            totalItemValue.Value -= pickUpItem.itemInfoData.purchaseCost;
            return pickUpItem;
        }

        public InventoryItem PickUpItem(InventoryItem pickUpItem)
        {
            if (pickUpItem == null) return null;

            CleanGridReference(pickUpItem);
            _itemPosDict.RemoveByKey(pickUpItem.itemInfoData);

            itemGridWeight.Value -= pickUpItem.itemInfoData.weight;
            totalItemValue.Value -= pickUpItem.itemInfoData.purchaseCost;
            return pickUpItem;
        }

        private InventoryItem PickUpFirstItem()
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    InventoryItem pickUpItem = PickUpItem(x, y);
                    if (pickUpItem) return pickUpItem;
                }
            }

            return null;
        }

        private void CleanGridReference(InventoryItem item)
        {
            for (int ix = 0; ix < item.Width; ix++)
            {
                for (int iy = 0; iy < item.Height; iy++)
                {
                    _inventoryItemSlot[item.onGridPositionX + ix, item.onGridPositionY + iy] = null;
                }
            }

            SubtractValue(item.itemInfoData.itemCode);
        }

        public bool BoundaryCheck(int posX, int posY, int width, int height)
        {
            if (!PositionCheck(posX, posY))
                return false;

            posX += width - 1;
            posY += height - 1;

            if (!PositionCheck(posX, posY))
                return false;

            return true;
        }

        private bool PositionCheck(int posX, int posY)
        {
            if (posX < 0 || posY < 0)
                return false;

            if (posX >= gridSize.x || posY >= gridSize.y)
                return false;

            return true;
        }

        [CanBeNull]
        public InventoryItem GetItem(int x, int y)
        {
            if (x < 0 || gridSize.x <= x) return null;
            if (y < 0 || gridSize.y <= y) return null;
            return _inventoryItemSlot[x, y];
        }

        private Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
        {
            int height = gridSize.y - itemToInsert.Height + 1;
            int width = gridSize.x - itemToInsert.Width + 1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (CheckAvailableSpace(x, y, itemToInsert.Width, itemToInsert.Height))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return null;
        }

        private bool CheckAvailableSpace(int posX, int posY, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (_inventoryItemSlot[posX + x, posY + y] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void AddValue(int key)
        {
            //Debug.Log("AddValue KEY" + key);
            if (!itemIdToCntDict.TryAdd(key, 1))
            {
                itemIdToCntDict[key] += 1;
            }
        }

        private void SubtractValue(int key)
        {
            if (itemIdToCntDict.ContainsKey(key))
            {
                // 키가 있으면 값을 -1 감소
                itemIdToCntDict[key] -= 1;

                // 값이 0이 되면 딕셔너리에서 키를 제거
                if (itemIdToCntDict[key] == 0)
                {
                    itemIdToCntDict.Remove(key);
                }
            }
            else
            {
                //Debug.LogWarning($"키 '{key}'가 딕셔너리에 없습니다.");
            }
        }

        public Dictionary<int, int> GetCurItemDictById()
        {
            return itemIdToCntDict;
        }

        public bool UseItem(InventoryItem pickUpItem)
        {
            if (pickUpItem == null) return true;

            ItemGrid selectItemGrid = null;
            if (pickUpItem.rotated) pickUpItem.Rotate();

            switch (pickUpItem.itemInfoData.itemType)
            {
                case ItemInfo.ItemType.Weapon:
                    selectItemGrid = GUIController.Instance.inventoryGUIManager.playerWeapon;
                    break;
                case ItemInfo.ItemType.Armor:
                    selectItemGrid = GUIController.Instance.inventoryGUIManager.playerArmor;
                    break;
                case ItemInfo.ItemType.Helmet:
                    selectItemGrid = GUIController.Instance.inventoryGUIManager.playerHelmet;
                    break;
                /*
                case ItemInfo.ItemType.Consumables:
                    // 소비 아이템은 바로 사용 후 성공 처리
                    PlayerManager pm = GameManager.Instance.GetPlayer();
                    pm.playerItemConsumeManager.UseItem(pickUpItem.itemInfoData.itemCode);
                    Destroy(pickUpItem.gameObject);
                    return true;
                    */
                case ItemInfo.ItemType.Misc:
                    // 단순 수집 아이템은 인벤토리에 들어갔을 경우만 성공 처리
                    return AddItem(pickUpItem.gameObject, false);
                /*
                case ItemInfo.ItemType.Blueprint:
                    // 던전 터렛 설계도의 경우 이미 가지고 있다면 실패 없다면 성공
                    if (WorldSaveGameManager.instance.currentGameData.dungeonResourceDic.TryAdd(
                            pickUpItem.itemInfoData.itemCode, true))
                    {
                        Debug.Log("Blueprint registered.");
                        Destroy(pickUpItem.gameObject);
                        WorldSaveGameManager.instance.SaveGame();
                        return true;
                    }

                    Debug.Log("Register Fail");
                    selectItemGrid = pickUpItem.previousItemGrid;
                    break;
                */
                default:
                    return false;
            }

            if (selectItemGrid == null) return true;

            // 1차 시도: 바로 장착 시도
            if (selectItemGrid.AddItem(pickUpItem.gameObject))
            {
                return true;
            }

            // 2차 시도: 기존 아이템 제거 후 장착 시도
            InventoryItem originItem = selectItemGrid.PickUpFirstItem();
            if (originItem == null)
            {
                // 기존에 아무것도 없는데도 실패 → 인벤토리로 복구
                AddItem(pickUpItem.gameObject, false);
                return false;
            }

            // 다시 장착 시도
            if (selectItemGrid.AddItem(pickUpItem.gameObject))
            {
                // 기존 아이템을 인벤토리에 넣기 시도
                if (AddItem(originItem.gameObject, false))
                {
                    return true; // 교체 장착 성공
                }
                else
                {
                    pickUpItem = selectItemGrid.PickUpFirstItem();
                    selectItemGrid.AddItem(originItem.gameObject, false);
                    AddItem(pickUpItem.gameObject, false);
                    return false;
                }
            }
            else
            {
                selectItemGrid.AddItem(originItem.gameObject, false);
                AddItem(pickUpItem.gameObject, false);
                return false;
            }
        }

        public virtual void ResetItemGrid()
        {
            // 1. 현재 그리드에 있는 모든 아이템들을 제거
            if (_inventoryItemSlot != null)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        InventoryItem item = _inventoryItemSlot[x, y];
                        if (item != null)
                        {
                            // 이미 처리된 아이템인지 확인 (같은 아이템이 여러 슬롯을 차지할 수 있음)
                            if (item.onGridPositionX == x && item.onGridPositionY == y)
                            {
                                Destroy(item.gameObject);
                            }
                        }
                    }
                }
            }

            // 2. 그리드 배열 초기화
            if (gridSize.x > 0 && gridSize.y > 0)
            {
                _inventoryItemSlot = new InventoryItem[gridSize.x, gridSize.y];
            }

            // 3. 아이템 위치 딕셔너리 초기화
            if (_itemPosDict != null)
            {
                _itemPosDict.Clear();
            }
            else
            {
                _itemPosDict = new Multimap<ItemInfo, Vector2>();
            }

            // 4. 아이템 카운트 딕셔너리 초기화
            if (itemIdToCntDict != null)
            {
                itemIdToCntDict.Clear();
            }
            else
            {
                itemIdToCntDict = new SerializedDictionary<int, int>();
            }

            // 5. 무게와 가치 초기화
            if (itemGridWeight != null)
            {
                itemGridWeight.Value = 0;
            }

            if (totalItemValue != null)
            {
                totalItemValue.Value = 0;
            }
        }

        #region HelperMethod

        private Dictionary<T, int> CountOccurrencesToDictionary<T>(List<T> sourceList) where T : notnull
        {
            // 1. GroupBy(): 리스트의 모든 요소를 자기 자신(Key)을 기준으로 그룹화합니다.
            // 2. ToDictionary(): 그룹화된 결과를 딕셔너리로 변환합니다.
            //    - Key: g.Key (그룹의 키, 즉 리스트의 고유한 값)
            //    - Value: g.Count() (그룹 내 요소의 개수)
            return sourceList
                .GroupBy(item => item)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count()
                );
        }

        #endregion

    }
}