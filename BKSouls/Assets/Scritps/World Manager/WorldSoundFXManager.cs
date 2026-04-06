using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Audio;

namespace BK
{
    public class WorldSoundFXManager : Singleton<WorldSoundFXManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmAudioSource;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxAudioSource;

        [Header("Mixer Group Parameters")]
        [SerializeField] private string masterVolumeParameter = "MasterVolume";
        [SerializeField] private string bgmVolumeParameter = "BGMVolume";
        [SerializeField] private string sfxVolumeParameter = "SFXVolume";
        
        [Header("Boss Track")]
        [SerializeField] AudioSource bossIntroPlayer;
        [SerializeField] AudioSource bossLoopPlayer;

        [SerializeField] private AudioClip emptySound;

        [Header("Damage Sounds")]
        public AudioClip[] physicalDamageSFX;

        [Header("Action Sounds")]
        public AudioClip pickUpItemSFX;
        public AudioClip rollSFX;
        public AudioClip stanceBreakSFX;
        public AudioClip criticalStrikeSFX;
        public AudioClip[] releaseArrowSFX;
        public AudioClip[] notchArrowSFX;
        public AudioClip healingFlaskSFX;
        
        private const float MIN_VOLUME = 0.0001f; // -80dB
        private const float MAX_VOLUME = 1f; // 0dB

        protected override void Awake()
        {
            base.Awake();
            ApplySavedVolumes();
        }

        private void ApplySavedVolumes()
        {
            audioMixer.SetFloat(masterVolumeParameter, ConvertToDecibels(GetMasterVolume()));
            audioMixer.SetFloat(bgmVolumeParameter, ConvertToDecibels(GetBGMVolume()));
            audioMixer.SetFloat(sfxVolumeParameter, ConvertToDecibels(GetSFXVolume()));
        }

        public AudioClip ChooseRandomSfxFromArray(AudioClip[] array)
        {
            if (array.Length == 0) return emptySound;
            int index = Random.Range(0, array.Length);

            return array[index];
        }

        public void PlayBossTrack(AudioClip introTrack, AudioClip loopTrack)
        {
            bossIntroPlayer.volume = 1;
            bossIntroPlayer.clip = introTrack;
            bossIntroPlayer.loop = false;
            bossIntroPlayer.Play();

            bossLoopPlayer.volume = 1;
            bossLoopPlayer.clip = loopTrack;
            bossLoopPlayer.loop = true;
            bossLoopPlayer.PlayDelayed(bossIntroPlayer.clip.length);
        }

        public void StopBossMusic()
        {
            StartCoroutine(FadeOutBossMusicThenStop());
        }

        private IEnumerator FadeOutBossMusicThenStop()
        {
            while (bossLoopPlayer.volume > 0)
            {
                bossLoopPlayer.volume -= Time.deltaTime;
                bossIntroPlayer.volume -= Time.deltaTime;
                yield return null;
            }

            bossIntroPlayer.Stop();
            bossLoopPlayer.Stop();
        }

        public void AlertNearbyCharactersToSound(Vector3 positionOfSound, float rangeOfSound)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            Collider[] characterColliders = Physics.OverlapSphere(positionOfSound, rangeOfSound);

            List<AICharacterManager> charactersToAlert = new List<AICharacterManager>();

            for (int i = 0; i < characterColliders.Length; i++)
            {
                AICharacterManager aiCharacter = characterColliders[i].GetComponent<AICharacterManager>();

                if (aiCharacter == null)
                    continue;

                if (charactersToAlert.Contains(aiCharacter))
                    continue;

                charactersToAlert.Add(aiCharacter);
            }

            for (int i = 0; i < charactersToAlert.Count; i++)
            {
                charactersToAlert[i].aiCharacterCombatManager.AlertCharacterToSound(positionOfSound);
            }
        }
        
        
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

            bgmAudioSource.clip = clip;
            bgmAudioSource.Play();
        }

        public void StopBGM()
        {
            bgmAudioSource.Stop();
        }
        
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxAudioSource == null) return;
            sfxAudioSource.PlayOneShot(clip);
        }

        public float GetMasterVolume() => PlayerPrefs.GetFloat("MasterVolume", 0.75f);

        public void SetMasterVolume(float normalizedVolume)
        {
            normalizedVolume = Mathf.Clamp01(normalizedVolume);
            audioMixer.SetFloat(masterVolumeParameter, ConvertToDecibels(normalizedVolume));
            PlayerPrefs.SetFloat("MasterVolume", normalizedVolume);
            PlayerPrefs.Save();
        }

        public float GetBGMVolume() => PlayerPrefs.GetFloat("BGMVolume", 0.75f);

        public void SetBGMVolume(float normalizedVolume)
        {
            normalizedVolume = Mathf.Clamp01(normalizedVolume);
            audioMixer.SetFloat(bgmVolumeParameter, ConvertToDecibels(normalizedVolume));
            PlayerPrefs.SetFloat("BGMVolume", normalizedVolume);
            PlayerPrefs.Save();
        }

        public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        public void SetSFXVolume(float normalizedVolume)
        {
            normalizedVolume = Mathf.Clamp01(normalizedVolume);
            audioMixer.SetFloat(sfxVolumeParameter, ConvertToDecibels(normalizedVolume));
            PlayerPrefs.SetFloat("SFXVolume", normalizedVolume);
            PlayerPrefs.Save();
        }

        private float ConvertToDecibels(float normalizedVolume)
        {
            normalizedVolume = Mathf.Max(normalizedVolume, MIN_VOLUME);
            return Mathf.Log10(normalizedVolume) * 20f;
        }
        
    }
}
