using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BK.Inventory
{
    public class InteractableShop : Interactable
    {
        [Header("Sale Item")]
        [SerializeField] protected ItemType saleItemType;
        public List<Item> saleItemList = new List<Item>();

        // 상점 초기화 상태 관리
        private ShopInitializationState initState = ShopInitializationState.NotInitialized;

        private enum ShopInitializationState
        {
            NotInitialized,
            InitializedWithItemList,
            InitializedWithLevel
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            // 상점이 준비되지 않았다면 초기화
            if (!IsShopReady())
            {
                InitializeShop();
            }

            EnterShop();
        }

        /// <summary>
        /// 상점이 사용할 준비가 되었는지 확인
        /// </summary>
        /// <returns>상점 사용 가능 여부</returns>
        private bool IsShopReady()
        {
            return initState != ShopInitializationState.NotInitialized && saleItemList.Count > 0;
        }

        protected void MarkShopInitialized()
        {
            initState = ShopInitializationState.InitializedWithItemList;
        }

        /// <summary>
        /// 현재 상태에 따라 적절한 방식으로 상점 초기화
        /// </summary>
        protected virtual void InitializeShop()
        {
            InitShopByLevel(WorldSaveGameManager.Instance.currentCharacterData.shelterLevel); 
        }


        /// <summary>
        /// 레벨을 기반으로 상점 초기화
        /// </summary>
        /// <param name="level">상점 레벨</param>
        private void InitShopByLevel(int level)
        {
            var availableItems = WorldItemDatabase.Instance.GetItemsByTypeAndTierRange(
                saleItemType,
                ItemTier.Common,
                (ItemTier)Mathf.Clamp(level, 0, Enum.GetValues(typeof(ItemTier)).Length - 1)
            );

            foreach (var originalItem in availableItems)
            {
                var shopItem = CreateShopItem(originalItem);
                saleItemList.Add(shopItem);
            }

            initState = ShopInitializationState.InitializedWithLevel;
            Debug.Log($"[InteractableShop] Initialized with level {level}. Items count: {saleItemList.Count}");
        }

        /// <summary>
        /// 원본 아이템을 기반으로 상점용 아이템 생성
        /// </summary>
        /// <param name="originalItem">원본 아이템</param>
        /// <returns>상점용 아이템</returns>
        private Item CreateShopItem(Item originalItem)
        {
            Item shopItem = Instantiate(originalItem);

            return shopItem;
        }

        /// <summary>
        /// 아이템으로 구매하는 경우의 비용 설정
        /// </summary>
        /// <param name="item">설정할 아이템</param>
        

        /// <summary>
        /// 판매 아이템 리스트 초기화
        /// </summary>
        protected void ClearSaleItems()
        {
            saleItemList.Clear();
        }

        /// <summary>
        /// 상점 진입
        /// </summary>
        protected virtual void EnterShop()
        {
            GUIController.Instance.OpenShop(saleItemList, this);
        }
    }
}
