using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class ScreenModeManager : MonoBehaviour
{
    [Header("해상도 설정")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    // 지원할 해상도 정의 (FHD, QHD, 4K)
    private readonly Resolution[] supportedResolutions = new Resolution[]
    {
        new Resolution { width = 1920, height = 1080, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } }, // FHD
        new Resolution { width = 2560, height = 1440, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } }, // QHD
        new Resolution { width = 3840, height = 2160, refreshRateRatio = new RefreshRate { numerator = 60, denominator = 1 } }  // 4K
    };

    private readonly string[] resolutionNames = { "FHD (1920x1080)", "QHD (2560x1440)", "4K (3840x2160)" };

    private void Start()
    {
        InitializeResolutionSettings();
        InitializeScreenModeSettings();
    }

    #region 해상도 설정
    private void InitializeResolutionSettings()
    {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // 지원하는 해상도 목록 추가
        for (int i = 0; i < supportedResolutions.Length; i++)
        {
            options.Add(resolutionNames[i]);

            // 현재 해상도와 가장 가까운 해상도 찾기
            if (supportedResolutions[i].width == Screen.currentResolution.width &&
                supportedResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        // 현재 해상도가 지원 목록에 없다면 가장 가까운 해상도 선택
        if (currentResolutionIndex == 0 && 
            !(Screen.currentResolution.width == 1920 && Screen.currentResolution.height == 1080))
        {
            currentResolutionIndex = GetClosestResolutionIndex();
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
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
        if (resolutionIndex >= 0 && resolutionIndex < supportedResolutions.Length)
        {
            Resolution resolution = supportedResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
    #endregion

    #region 화면 모드 설정
    private void InitializeScreenModeSettings()
    {
        // 화면 모드 옵션 설정
        screenModeDropdown.ClearOptions();
        
        List<string> screenModeOptions = new List<string>
        {
            "전체화면",
            "테두리없는 창모드 전체화면",
            "창모드"
        };
        
        screenModeDropdown.AddOptions(screenModeOptions);
        
        // 현재 화면 모드 설정
        int currentScreenMode = 0;
        if (Screen.fullScreen)
        {
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            {
                currentScreenMode = 0; // 전체화면
            }
            else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
            {
                currentScreenMode = 1; // 테두리없는 창모드 전체화면
            }
        }
        else
        {
            currentScreenMode = 2; // 창모드
        }
        
        screenModeDropdown.value = currentScreenMode;
        screenModeDropdown.RefreshShownValue();
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    public void SetScreenMode(int screenModeIndex)
    {
        switch (screenModeIndex)
        {
            case 0: // 전체화면
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // 테두리없는 창모드 전체화면
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: // 창모드
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }
    #endregion
}