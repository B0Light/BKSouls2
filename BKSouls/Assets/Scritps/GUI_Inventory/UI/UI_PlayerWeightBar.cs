using System;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{

    public class UI_PlayerWeightBar : MonoBehaviour
    {
        [SerializeField] private Image weightIcon;
        [SerializeField] private Image weightBar;
        [SerializeField] private Image weightBarColor;

        [Header("Sprite Color")] [SerializeField]
        private Color colorSafe;

        [SerializeField] private Color colorDanger;

        private void OnEnable()
        {
            WorldPlayerInventory.Instance.itemWeight.OnValueChanged += UpdateWeightBar;

            UpdateWeightBar(WorldPlayerInventory.Instance.itemWeight.Value);
        }

        private void OnDisable()
        {
            WorldPlayerInventory.Instance.itemWeight.OnValueChanged -= UpdateWeightBar;
        }

        private void UpdateWeightBar(float newValue)
        {
            float fill = newValue / WorldPlayerInventory.Instance.itemWeight.MaxValue;
            weightBar.fillAmount = fill;
            UpdateIconColor(fill);
        }

        private void UpdateIconColor(float value)
        {
            weightBarColor.color = Color.Lerp(colorSafe, colorDanger, value);

            weightIcon.color = value < 1 ? colorSafe : colorDanger;
        }
    }
}
