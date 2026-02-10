using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BK
{
    public class UI_StatBar : MonoBehaviour
    {
        protected Slider slider;
        protected RectTransform rectTransform;

        [Header("Bar Options")]
        [SerializeField] protected bool scaleBarLengthWithStats = true;
        [SerializeField] protected float widthScaleMultiplier = 1;
        // SECONDARY BAR BEHIND MAY BAR FOR POLISH EFFECT (YELLOW BAR THAT SHOWS HOW MUCH AN ACTION/DAMAGE TAKES AWAY FROM CURRENT STAT)

        [Header("Fill Color")]
        [SerializeField] protected Image fillImage;
        [SerializeField] protected Color barFillColor;
        
        protected virtual void Awake()
        {
            slider = GetComponent<Slider>();
            rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Start()
        {

        }

        public virtual void SetStat(int newValue)
        {
            slider.value = newValue;
        }

        public virtual void SetMaxStat(int maxValue)
        {
            slider.maxValue = maxValue;
            slider.value = maxValue;

            if (scaleBarLengthWithStats)
            {
                rectTransform.sizeDelta = new Vector2(maxValue * widthScaleMultiplier, rectTransform.sizeDelta.y);

                //  RESETS THE POSITION OF THE BARS BASED ON THEIR LAYOUT GROUP'S SETTINGS
                GUIController.Instance.playerUIHudManager.RefreshHUD();
            }
        }
        
        public void ToggleBarFillColor(bool isPoisoned)
        {
            if (fillImage == null)
                return;

            if (isPoisoned)
            {
                fillImage.color = WorldUtilityManager.Instance.GetPoisonedColor();
            }
            else
            {
                fillImage.color = barFillColor;
            }
        }
    }
}