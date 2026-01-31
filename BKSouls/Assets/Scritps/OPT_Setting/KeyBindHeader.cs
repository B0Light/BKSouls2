using TMPro;
using UnityEngine;

public class KeyBindHeader : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;

    public void Initialize(string text)
    {
        title.text = text;
    }
}
