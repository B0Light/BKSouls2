using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BK
{
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
        [SerializeField] private float sfxPreviewDelay = 0.1f;

        private float _sfxPreviewTimer = 0f;
        private bool _sfxPreviewPending = false;

        private void Start()
        {
            InitializeSliders();
            SetupListeners();
        }

        private void Update()
        {
            if (_sfxPreviewPending)
            {
                _sfxPreviewTimer -= Time.deltaTime;
                if (_sfxPreviewTimer <= 0f)
                {
                    _sfxPreviewPending = false;
                    if (testSFX != null && WorldSoundFXManager.Instance != null)
                        WorldSoundFXManager.Instance.PlaySfx(testSFX);
                }
            }
        }

        private void InitializeSliders()
        {
            if (WorldSoundFXManager.Instance == null) return;

            masterVolumeSlider.value = WorldSoundFXManager.Instance.GetMasterVolume();
            bgmVolumeSlider.value = WorldSoundFXManager.Instance.GetBGMVolume();
            sfxVolumeSlider.value = WorldSoundFXManager.Instance.GetSFXVolume();

            UpdateVolumeTexts();
        }

        private void SetupListeners()
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (WorldSoundFXManager.Instance == null) return;
            WorldSoundFXManager.Instance.SetMasterVolume(value);
            UpdateVolumeTexts();
        }

        private void OnBGMVolumeChanged(float value)
        {
            if (WorldSoundFXManager.Instance == null) return;
            WorldSoundFXManager.Instance.SetBGMVolume(value);
            UpdateVolumeTexts();
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (WorldSoundFXManager.Instance == null) return;
            WorldSoundFXManager.Instance.SetSFXVolume(value);
            UpdateVolumeTexts();

            _sfxPreviewTimer = sfxPreviewDelay;
            _sfxPreviewPending = true;
        }

        private void UpdateVolumeTexts()
        {
            if (masterVolumeText != null)
                masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";

            if (bgmVolumeText != null)
                bgmVolumeText.text = $"{Mathf.RoundToInt(bgmVolumeSlider.value * 100)}%";

            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
        }

        public void PlayTestSFX()
        {
            if (WorldSoundFXManager.Instance != null && testSFX != null)
                WorldSoundFXManager.Instance.PlaySfx(testSFX);
        }
    }
}
