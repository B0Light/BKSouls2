using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class HUD_SelectedItemInfo : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image selectedItemImage;
        [SerializeField] private Image iconBackgroundImage01;
        [SerializeField] private Image iconBackgroundImage02;
        [SerializeField] private Image backgroundImage01;
        [SerializeField] private Image backgroundImage02;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI itemDurability;
        [SerializeField] private TextMeshProUGUI itemGold;
        [SerializeField] private TextMeshProUGUI itemWeight;
        [SerializeField] private Transform abilityContainer; // 모든 능력이 들어갈 컨테이너

        [SerializeField] private GameObject abilityUIPrefab; // HUD_SelectedItemAbility 프리팹

        private List<HUD_SelectedItemAbility> abilityUIs = new List<HUD_SelectedItemAbility>();

        private String _itemDescription;

        public void Init(ItemInfo itemInfo)
        {
            canvasGroup.alpha = 1;
            EquipmentItemInfo equipmentItemInfo = itemInfo as EquipmentItemInfo;
            EquipmentItemInfoWeapon itemInfoWeapon = equipmentItemInfo as EquipmentItemInfoWeapon;
            selectedItemImage.sprite = itemInfoWeapon ? itemInfoWeapon.weaponEquipSprite : itemInfo.itemIcon;

            iconBackgroundImage01.color = WorldDatabase_Item.Instance.GetItemBackgroundColorByTier(itemInfo.itemTier);
            iconBackgroundImage02.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemInfo.itemTier);
            backgroundImage01.color = WorldDatabase_Item.Instance.GetItemBackgroundColorByTier(itemInfo.itemTier);
            backgroundImage02.color = WorldDatabase_Item.Instance.GetItemColorByTier(itemInfo.itemTier);
            itemName.text = itemInfo.itemName;

            _itemDescription = itemInfo.itemDescription;
            itemWeight.text = itemInfo.weight.ToString("F1");
            itemGold.text = itemInfo.purchaseCost.ToString();

            // 기존 능력 UI들 정리
            ClearAbilities();

            // 추가 능력들 생성
            if (itemInfo.itemAbilities != null && itemInfo.itemAbilities.Count > 0)
            {
                foreach (ItemAbility ability in itemInfo.itemAbilities)
                {
                    CreateAbilityFromItemAbility(ability);
                }
            }
        }

        private void CreateAbilityFromItemAbility(ItemAbility ability)
        {
            if (abilityUIPrefab == null || abilityContainer == null) return;

            GameObject abilityObj = Instantiate(abilityUIPrefab, abilityContainer);
            HUD_SelectedItemAbility abilityComponent = abilityObj.GetComponent<HUD_SelectedItemAbility>();

            if (abilityComponent != null)
            {
                // icon / value
                abilityComponent.Init_ability(ability);

                if (ability.itemEffect == ItemEffect.Resource)
                {
                    abilityComponent.SetText(_itemDescription);
                }

                abilityUIs.Add(abilityComponent);
            }
        }

        private void ClearAbilities()
        {
            foreach (var ability in abilityUIs)
            {
                if (ability != null && ability.gameObject != null)
                    DestroyImmediate(ability.gameObject);
            }

            abilityUIs.Clear();
        }

        private void OnDestroy()
        {
            ClearAbilities();
        }
    }
}