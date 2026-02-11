using UnityEngine;
using TMPro;

namespace BK
{
    public class UI_StatusEffectWarning : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI warningText;
        public CanvasGroup canvas;

        [Header("Effect Colors")]
        [SerializeField] Color poisonedColor;
        [SerializeField] Color bloodLossColor;
        [SerializeField] Color frostColor;

        public void SetWarningMessage(BuildUp status)
        {
            switch (status)
            {
                case BuildUp.Poison:
                    warningText.color = poisonedColor;
                    warningText.text = "POISONED!";
                    break;
                case BuildUp.Bleed:
                    warningText.color = bloodLossColor;
                    warningText.text = "BLOOD LOSS!";
                    break;
                case BuildUp.Frost:
                    warningText.color = frostColor;
                    warningText.text = "FROSTBITE!";
                    break;
                default:
                    break;
            }
        }
    }
}