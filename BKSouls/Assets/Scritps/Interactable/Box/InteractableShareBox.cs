using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;

namespace BK
{
    public class InteractableShareBox : InteractableBox
    {
        // is Open : 최초 오픈시 약간의 대기 시간을 주기 위한 코드
        // isOpened : 현재 오브젝트의 문이 열여있는지 확인
        protected override void Start()
        {
            base.Awake();
            itemIdList = new List<int>();
        }

        protected override void PerformInteraction()
        {
            itemIdList.Clear();

            foreach (var pair in WorldSaveGameManager.Instance.currentCharacterData.shareInventoryItems)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    itemIdList.Add(pair.Key);
                }
            }

            GUIController.Instance.OpenShareBox(
                WorldSaveGameManager.Instance.currentCharacterData.shareBoxSize.x,
                WorldSaveGameManager.Instance.currentCharacterData.shareBoxSize.y,
                itemIdList, this);
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();

            WorldSaveGameManager.Instance.currentCharacterData.shareInventoryItems.Clear();
            foreach (var pair in WorldPlayerInventory.Instance.GetShareInventory().GetCurItemDictById())
            {
                WorldSaveGameManager.Instance.currentCharacterData.shareInventoryItems.Add(pair.Key, pair.Value);
            }
        }

        public override void SetToSpecificLevel(int level)
        {
            int[] yValues = { 20, 25, 30, 40, 50, 70 };
            int ySize = (level >= 0 && level < yValues.Length) ? yValues[level] : 70;

            WorldSaveGameManager.Instance.currentCharacterData.shareBoxSize = new Vector2Int(8, ySize);
            WorldSaveGameManager.Instance.SaveGame();
        }
    }
}