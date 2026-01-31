using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class KeyBindingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonSlot;
    
    private Dictionary<int, GameObject> bindingButtons = new();

    private System.Action onRebindClick;

    public void Initialize(string actionName)
    {
        actionNameText.text = actionName;
    }

    public void AddButton(int bindIndex, string keyName, System.Action onRebindClick)
    {
        GameObject iButton = Instantiate(buttonPrefab, buttonSlot);
        bindingButtons[bindIndex] = iButton; // 인덱스로 저장

        Button btn = iButton.GetComponent<Button>();
        TextMeshProUGUI keyNameText = iButton.GetComponentInChildren<TextMeshProUGUI>();
        keyNameText.text = keyName;

        btn.onClick.AddListener(() => onRebindClick?.Invoke());
    }

    
    public void UpdateKeyText(int bindIndex, string keyName)
    {
        if (!bindingButtons.TryGetValue(bindIndex, out GameObject button) || button == null)
        {
            Debug.LogWarning($"ERROR: No button found for index {bindIndex}");
            return;
        }

        button.GetComponentInChildren<TextMeshProUGUI>()?.SetText(keyName);
    }
}