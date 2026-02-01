using UnityEngine;

namespace BK
{
    public class UI_Character_Attribute_Slider : MonoBehaviour
    {
        [SerializeField] CharacterAttribute sliderAttribute;

        public void SetCurrentSelectedAttribute()
        {
            GUIController.Instance.playerUILevelUpManager.currentSelectedAttribute = sliderAttribute;
        }
    }
}
