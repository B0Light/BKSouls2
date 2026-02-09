using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class HUD_SelectedItemAbility : MonoBehaviour
    {
        [SerializeField] private Image abilityIcon;
        [SerializeField] private TextMeshProUGUI abilityText;

        public void SetText(string str)
        {
            abilityText.text = str;
        }

        public void Init_ability(ItemAbility ability)
        {
            Init(ability.itemEffect, ability.value);
        }

        // 통합된 초기화 메서드
        private void Init(ItemEffect effect, int value)
        {
            abilityIcon.sprite = WorldItemDatabase.Instance.GetDefaultIcon(effect);

            // 텍스트 설정
            abilityText.text = GetEffectText(effect, value);
        }

        private string GetEffectText(ItemEffect effect, int value)
        {
            switch (effect)
            {
                case ItemEffect.PhysicalAttack:
                    return $"<color=#e74c3c><b>+{value}</b></color> 물리 공격력"; // 붉은 계열

                case ItemEffect.MagicalAttack:
                    return $"<color=#9b59b6><b>+{value}</b></color> 마법 공격력"; // 보라

                case ItemEffect.PhysicalDefense:
                    return $"<color=#3498db><b>+{value}%</b></color> 물리 방어력"; // 파랑

                case ItemEffect.MagicalDefense:
                    return $"<color=#8e44ad><b>+{value}%</b></color> 마법 방어력"; // 보라 + 파랑 계열

                case ItemEffect.HealthPoint:
                    return $"<color=#e67e22><b>+{value}</b></color> 체력"; // 주황

                case ItemEffect.BuffAttack:
                    return $"<color=#e74c3c>+{value}%</color> 공격력 증가"; // 붉은 계열

                case ItemEffect.BuffDefense:
                    return $"<color=#3498db>+{value}%</color> 방어력 증가"; // 파랑 계열

                case ItemEffect.BuffActionPoint:
                    return $"<color=#1abc9c>+{value}%</color> 행동력 증가"; // 청록

                case ItemEffect.UtilitySpeed:
                    return $"<color=#27ae60>+{value}%</color> 이동속도 증가"; // 초록

                case ItemEffect.UtilityWeight:
                    return $"<color=#f1c40f>+{value}%</color> 무게 감소"; // 노랑

                case ItemEffect.RestoreHealth:
                    return $"<color=#2ecc71><b>+{value}</b></color> 체력 회복"; // 녹색

                case ItemEffect.Resource:
                    return " 분명 어딘가 쓸모가 있을 것입니다.";

                case ItemEffect.StorageSpace:
                    return $"<color=#95a5a6><b>+{value}</b></color> 배낭 공간"; // 회색 계열

                default:
                    return "분명 어딘가 쓸모가 있을 것입니다.";
            }
        }
    }
}