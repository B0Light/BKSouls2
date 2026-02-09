using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace BK
{
    public class UI_BuildUpBar : UI_StatBar
    {
        public BuildUp buildUpType;

        public override void SetMaxStat(int maxValue)
        {
            slider.maxValue = maxValue;

            if (scaleBarLengthWithStats)
            {
                rectTransform.sizeDelta = new Vector2(maxValue * widthScaleMultiplier, rectTransform.sizeDelta.y);

                //  RESETS THE POSITION OF THE BARS BASED ON THEIR LAYOUT GROUP'S SETTINGS
                GUIController.Instance.playerUIHudManager.RefreshHUD();
            }

            if (slider.value <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public override void SetStat(int newValue)
        {
            base.SetStat(newValue);

            if (slider.value <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
