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

        public void Init(int itemDataID, int itemCount)
        {
            ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(itemDataID);
            itemIcon.sprite = itemInfoData.itemIcon;
            itemFrame.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemInfoData.itemTier);

            if (itemNameText)
                itemNameText.text = itemInfoData.itemName;
            if (itemCntText)
            {
                int inventoryCnt = WorldPlayerInventory.Instance.GetItemCountInAllInventory(itemDataID);
                itemCntText.text = itemCount + " / " + inventoryCnt;
                itemCntText.color = itemCount > inventoryCnt
                    ? new Color(1f, 0.6f, 0.6f) // 파스텔 레드
                    : new Color(0.6f, 1f, 0.6f); // 파스텔 그린
            }
        }
    }
}