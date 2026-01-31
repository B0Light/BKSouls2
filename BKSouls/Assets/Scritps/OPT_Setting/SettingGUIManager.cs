using System;
using TMPro;
using UnityEngine;

namespace BK
{
    public class SettingGUIManager : GUIComponent
    {
        private enum OptionType
        {
            Display,
            KeyBind,
            Sound,
            Exit,
            None,
        }

        [SerializeField] private TextMeshProUGUI navigationText;

        [SerializeField] private CanvasGroup optionsPanel;
        [SerializeField] private CanvasGroup displaySetter;
        [SerializeField] private CanvasGroup keyBinder;
        [SerializeField] private CanvasGroup soundController;
        [SerializeField] private CanvasGroup gameExit;

        private OptionType _curActiveType;

        private void Start()
        {
            HideAllSetting();
            _curActiveType = OptionType.Display;
        }

        public void OpenDisplaySetter()
        {
            HideAllSetting();
            ToggleSetting(displaySetter, true);
            _curActiveType = OptionType.Display;
            SetNavigateText();
        }

        public void OpenKeyBinder()
        {
            HideAllSetting();
            ToggleSetting(keyBinder, true);
            _curActiveType = OptionType.KeyBind;
            SetNavigateText();
        }

        public void OpenSoundController()
        {
            HideAllSetting();
            ToggleSetting(soundController, true);
            _curActiveType = OptionType.Sound;
            SetNavigateText();
        }

        public void OpenExit()
        {
            HideAllSetting();
            ToggleSetting(gameExit, true);
            _curActiveType = OptionType.Exit;
            SetNavigateText();
        }

        private void ToggleSetting(CanvasGroup canvasGroup, bool value)
        {
            canvasGroup.alpha = value ? 1 : 0;
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }

        private void HideAllSetting()
        {
            ToggleSetting(displaySetter, false);
            ToggleSetting(keyBinder, false);
            ToggleSetting(soundController, false);
            ToggleSetting(gameExit, false);
            _curActiveType = OptionType.None;
        }

        private void SetNavigateText()
        {
            string setText = "";
            switch (_curActiveType)
            {
                case OptionType.Display:
                    setText = "Display";
                    break;
                case OptionType.KeyBind:
                    setText = "Key Bind";
                    break;
                case OptionType.Sound:
                    setText = "Sound";
                    break;
                case OptionType.Exit:
                    setText = "Exit";
                    break;
                default:
                    break;
            }

            navigationText.text = setText;
        }
    }
}