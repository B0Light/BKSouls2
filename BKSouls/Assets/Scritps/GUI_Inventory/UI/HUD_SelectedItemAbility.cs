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

                case ItemEffect.RestoreHealth:
                    return $"<color=#2ecc71><b>+{value}</b></color> 체력 회복"; // 녹색

                case ItemEffect.FireDamage:
                    return $"<color=#e74c3c>+{value}%</color> 화염 피해"; // 붉은 계열

                case ItemEffect.IceDamage:
                    return $"<color=#3498db>+{value}%</color> 냉기 피해"; // 파랑 계열

                case ItemEffect.LightningDamage:
                    return $"<color=#f1c40f>+{value}%</color> 번개 피해"; // 노랑 계열

                case ItemEffect.BleedDamage:
                    return $"<color=#c0392b>+{value}%</color> 출혈 피해"; // 어두운 붉은 계열

                default:
                    return "분명 어딘가 쓸모가 있을 것입니다.";
            }
        }
    }
}