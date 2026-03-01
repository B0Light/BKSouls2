using UnityEngine;
using TMPro;

public class IncomeLogUI : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    public void Setup(string message)
    {
        logText.text = message;
    }
}