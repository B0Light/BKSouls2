using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ShopCostItem : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image itemFrame;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemCntText;

        public void Init(GridItem itemInfoData, int itemCount)
        {
            if (itemInfoData == null)
            {
                Debug.LogWarning("[ShopCostItem] itemInfoData is null.");
                return;
            }

            itemIcon.sprite = itemInfoData.itemIcon ?? WorldItemDatabase.Instance.unknownIcon;
            itemFrame.color = WorldItemDatabase.Instance.GetItemColorByTier(itemInfoData.itemTier);

            if (itemNameText)
                itemNameText.text = itemInfoData.itemName;
            if (itemCntText)
            {
                int inventoryCnt = WorldPlayerInventory.Instance.GetItemCountInAllInventory(itemInfoData.itemID);
                itemCntText.text = inventoryCnt + " / " + itemCount;
                itemCntText.color = itemCount > inventoryCnt
                    ? new Color(1f, 0.6f, 0.6f) // 파스텔 레드
                    : new Color(0.6f, 1f, 0.6f); // 파스텔 그린
            }
        }
        public void InitCost(GridItem itemInfoData, int cost, int currentAmount)
        {
            if (itemInfoData == null)
            {
                Debug.LogWarning("[ShopCostItem] itemInfoData is null.");
                return;
            }

            itemIcon.sprite = itemInfoData.itemIcon ?? WorldItemDatabase.Instance.unknownIcon;
            itemFrame.color = WorldItemDatabase.Instance.GetItemColorByTier(itemInfoData.itemTier);

            if (itemNameText)
                itemNameText.text = itemInfoData.itemName;

            if (itemCntText)
            {
                itemCntText.text = cost.ToString();
                itemCntText.color = cost > currentAmount
                    ? new Color(1f, 0.6f, 0.6f)
                    : new Color(0.6f, 1f, 0.6f);
            }
        }
    }
}
