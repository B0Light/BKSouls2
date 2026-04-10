using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class CharacterSoundFXManager : MonoBehaviour
    {
        private AudioSource audioSource;

        [Header("Damage Grunts")]
        [SerializeField] protected AudioClip[] damageGrunts;

        [Header("Attack Grunts")]
        [SerializeField] protected AudioClip[] attackGrunts;

        [Header("FootSteps")]
        [SerializeField] protected AudioClip[] footSteps;

        protected virtual void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            if (WorldSoundFXManager.Instance != null)
                audioSource.outputAudioMixerGroup = WorldSoundFXManager.Instance.GetSFXMixerGroup();
        }

        public void PlaySoundFX(AudioClip soundFX, float volume = 1, bool randomizePitch = true, float pitchRandom = 0.1f)
        {
            audioSource.PlayOneShot(soundFX, volume);
            //  RESETS PITCH
            audioSource.pitch = 1;

            if (randomizePitch)
            {
                audioSource.pitch += Random.Range(-pitchRandom, pitchRandom);
            }
        }

        public void PlayRollSoundFX()
        {
            audioSource.PlayOneShot(WorldSoundFXManager.Instance.rollSFX);
        }

        public virtual void PlayDamageGruntSoundFX()
        {
            if (damageGrunts.Length > 0)
                PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(damageGrunts));
        }

        public virtual void PlayAttackGruntSoundFX()
        {
            if (attackGrunts.Length > 0)
                PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(attackGrunts));
        }

        public virtual void PlayFootStepSoundFX()
        {
            if (footSteps.Length > 0)
                PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(footSteps));
        }

        public virtual void PlayStanceBreakSoundFX()
        {
            audioSource.PlayOneShot(WorldSoundFXManager.Instance.stanceBreakSFX);
        }

        public virtual void PlayCriticalStrikeSoundFX()
        {
            audioSource.PlayOneShot(WorldSoundFXManager.Instance.criticalStrikeSFX);
        }

        public virtual void PlayBlockSoundFX()
        {

        }
    }
}
