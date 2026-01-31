using UnityEngine;
using TMPro;

namespace BK.Inventory
{
    public class IncomeLogUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;

        public void Setup(string message)
        {
            logText.text = message;
        }
    }
}