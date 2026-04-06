using TMPro;
using UnityEngine;

namespace BK
{
    public class KeyBindHeader : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;

        public void Initialize(string text)
        {
            title.text = text;
        }
    }
}
