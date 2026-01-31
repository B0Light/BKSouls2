using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundOptionsUI : MonoBehaviour
{
    [Header("Slider References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Text References (Optional)")]
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;

    [Header("Test Audio")]
    [SerializeField] private AudioClip testSFX;

    /*
   private void Start()
   {
       InitializeSliders();
       SetupListeners();
   }


   private void InitializeSliders()
   {
       // Initialize sliders with saved values
       if (WorldSoundFXManager.Instance != null)
       {
           masterVolumeSlider.value = WorldSoundFXManager.Instance.GetMasterVolume();
           bgmVolumeSlider.value = WorldSoundFXManager.Instance.GetBGMVolume();
           sfxVolumeSlider.value = WorldSoundFXManager.Instance.GetSFXVolume();

           UpdateVolumeTexts();
       }
   }

   private void SetupListeners()
   {
       // Set up slider event listeners
       masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
       bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
       sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
   }

   private void OnMasterVolumeChanged(float value)
   {
       if (WorldSoundFXManager.Instance != null)
       {
           WorldSoundFXManager.Instance.SetMasterVolume(value);
           UpdateVolumeTexts();
       }
   }

   private void OnBGMVolumeChanged(float value)
   {
       if (WorldSoundFXManager.Instance != null)
       {
           WorldSoundFXManager.Instance.SetBGMVolume(value);
           UpdateVolumeTexts();
       }
   }

   private void OnSFXVolumeChanged(float value)
   {
       if (WorldSoundFXManager.Instance != null)
       {
           WorldSoundFXManager.Instance.SetSfxVolume(value);
           UpdateVolumeTexts();

           // Optional: Play test sound when adjusting SFX volume
           if (testSFX != null)
           {
               WorldSoundFXManager.Instance.PlaySfx(testSFX);
           }
       }
   }

   private void UpdateVolumeTexts()
   {
       // Update volume text displays if available
       if (masterVolumeText != null)
       {
           masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
       }

       if (bgmVolumeText != null)
       {
           bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolumeSlider.value * 100)}%";
       }

       if (sfxVolumeText != null)
       {
           sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
       }
   }

   public void PlayTestSFX()
   {
       if (WorldSoundFXManager.Instance != null && testSFX != null)
       {
           WorldSoundFXManager.Instance.PlaySfx(testSFX);
       }
   }
   */
}