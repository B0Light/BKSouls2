using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace BK.Inventory
{
    public class HUD_CashText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cashText;

        private void OnEnable()
        {
            StartCoroutine(InitializeWithTimeout(5f));
        }

        private IEnumerator InitializeWithTimeout(float timeout)
        {
            float elapsedTime = 0f;

            while (WorldPlayerInventory.Instance == null && elapsedTime < timeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (WorldPlayerInventory.Instance == null) yield break;

            WorldPlayerInventory.Instance.balance.OnValueChanged += SetBalanceText;

            SetBalanceText(WorldPlayerInventory.Instance.balance.Value);
        }

        private void SetBalanceText(int value)
        {
            cashText.text = value.ToString();
        }
    }
}