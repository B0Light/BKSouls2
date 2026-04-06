using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace BK
{
    public class ScreenModeManager : MonoBehaviour
    {
        [Header("해상도 설정")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown screenModeDropdown;

        private readonly Resolution[] supportedResolutions = new Resolution[]
        {
            new Resolution { width = 1920, height = 1080, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } },
            new Resolution { width = 2560, height = 1440, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } },
            new Resolution { width = 3840, height = 2160, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } }
        };

        private readonly string[] resolutionNames = { "FHD (1920x1080)", "QHD (2560x1440)", "4K (3840x2160)" };

        private const string PREF_RESOLUTION = "ResolutionIndex";
        private const string PREF_SCREEN_MODE = "ScreenModeIndex";

        private void Start()
        {
            InitializeResolutionSettings();
            InitializeScreenModeSettings();
        }

        #region 해상도 설정

        private void InitializeResolutionSettings()
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(new List<string>(resolutionNames));

            int savedIndex = PlayerPrefs.GetInt(PREF_RESOLUTION, -1);
            resolutionDropdown.value = savedIndex >= 0 && savedIndex < supportedResolutions.Length
                ? savedIndex
                : GetClosestResolutionIndex();

            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        private int GetClosestResolutionIndex()
        {
            int closestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < supportedResolutions.Length; i++)
            {
                float distance = Mathf.Abs(supportedResolutions[i].width - Screen.currentResolution.width) +
                                 Mathf.Abs(supportedResolutions[i].height - Screen.currentResolution.height);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        public void SetResolution(int resolutionIndex)
        {
            if (resolutionIndex < 0 || resolutionIndex >= supportedResolutions.Length) return;

            Resolution res = supportedResolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt(PREF_RESOLUTION, resolutionIndex);
            PlayerPrefs.Save();
        }

        #endregion

        #region 화면 모드 설정

        private void InitializeScreenModeSettings()
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new List<string> { "전체화면", "테두리없는 창모드 전체화면", "창모드" });

            int savedIndex = PlayerPrefs.GetInt(PREF_SCREEN_MODE, -1);
            screenModeDropdown.value = savedIndex >= 0 ? savedIndex : GetCurrentScreenModeIndex();

            screenModeDropdown.RefreshShownValue();
            screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        }

        private int GetCurrentScreenModeIndex()
        {
            return Screen.fullScreenMode switch
            {
                FullScreenMode.ExclusiveFullScreen => 0,
                FullScreenMode.FullScreenWindow => 1,
                _ => 2,
            };
        }

        public void SetScreenMode(int screenModeIndex)
        {
            Screen.fullScreenMode = screenModeIndex switch
            {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                _ => FullScreenMode.Windowed,
            };
            PlayerPrefs.SetInt(PREF_SCREEN_MODE, screenModeIndex);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
