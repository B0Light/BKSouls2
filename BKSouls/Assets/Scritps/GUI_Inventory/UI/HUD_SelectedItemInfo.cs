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

        [SerializeField] private GameObject[] abilityUIObjects; 
        private int _currentAbilityIndex = 0;

        private String _itemDescription;

        public void Init(GridItem itemInfo)
        {
            canvasGroup.alpha = 1;
            selectedItemImage.sprite = itemInfo.itemIcon;

            iconBackgroundImage01.color = WorldItemDatabase.Instance.GetItemBackgroundColorByTier(itemInfo.itemTier);
            iconBackgroundImage02.color = WorldItemDatabase.Instance.GetItemColorByTier(itemInfo.itemTier);
            backgroundImage01.color = WorldItemDatabase.Instance.GetItemBackgroundColorByTier(itemInfo.itemTier);
            backgroundImage02.color = WorldItemDatabase.Instance.GetItemColorByTier(itemInfo.itemTier);
            itemName.text = itemInfo.itemName;

            _itemDescription = itemInfo.itemDescription;
            itemWeight.text = itemInfo.weight.ToString("F1");
            itemGold.text = itemInfo.cost.ToString();

            // 기존 능력 UI들 정리
            ClearAbilities();

            _currentAbilityIndex = 0;
            // 추가 능력들 생성
            if (itemInfo.itemAbilities != null)
            {
                foreach (ItemAbility ability in itemInfo.itemAbilities)
                {
                    // 배열 범위 안에서만 실행되도록 안전장치 추가
                    if (_currentAbilityIndex < abilityUIObjects.Length)
                    {
                        CreateAbilityFromItemAbility(ability, abilityUIObjects[_currentAbilityIndex]);
                        _currentAbilityIndex++; // 다음 호출을 위해 인덱스 증가
                    }
                }
            }
        }

        private void CreateAbilityFromItemAbility(ItemAbility ability, GameObject targetObject)
        {
            targetObject.SetActive(true);
            HUD_SelectedItemAbility abilityComponent = targetObject.GetComponent<HUD_SelectedItemAbility>();

            if (abilityComponent != null)
            {
                abilityComponent.Init_ability(ability);

                if (ability.itemEffect == ItemEffect.Resource)
                {
                    abilityComponent.SetText(_itemDescription);
                }
            }
        }

        private void ClearAbilities()
        {
            foreach (var ability in abilityUIObjects)
            {
                ability.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            ClearAbilities();
        }
    }
}