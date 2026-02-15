using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class WorldCharacterEffectsManager : Singleton<WorldCharacterEffectsManager>
    {
        [Header("VFX")]
        public GameObject bloodSplatterVFX;
        public GameObject criticalBloodSplatterVFX;
        public GameObject healingFlaskVFX;
        public GameObject deadSpotVFX;
        public GameObject poisonedVFX;
        public GameObject bloodLossVFX;
        public GameObject frostBiteVFX;

        [Header("Damage")]
        public TakeDamageEffect takeDamageEffect;
        public TakeBlockedDamageEffect takeBlockedDamageEffect;
        public TakeCriticalDamageEffect takeCriticalDamageEffect;
        
        [Header("Frost Bite")]
        public ModifyStaminaRegenerationForATimeEffect frostBiteStaminaRegenerationEffect;

        [Header("Status Effects")]
        public PoisonedEffect poisonedEffect;
        public BloodLossEffect bloodLossEffect;
        public FrostBiteEffect frostBiteEffect;

        [Header("Take Build Ups")]
        public TakeBuildUpEffect takePoisonBuildUpEffect;
        public TakeBuildUpEffect takeBleedBuildUpEffect;
        public TakeBuildUpEffect takeFrostBuildUpEffect;

        [Header("Degrade Build Ups")]
        public BuildUpEffect degradePoisonBuildUpEffect;
        public BuildUpEffect degradeBleedBuildUpEffect;
        public BuildUpEffect degradeFrostBiteBuildUpEffect;

        [Header("Two Hand")]
        public TwoHandingEffect twoHandingEffect;

        [Header("Instant Effects")]
        [SerializeField] List<InstantCharacterEffect> instantEffects;

        [Header("Static Effects")]
        [SerializeField] List<StaticCharacterEffect> staticEffects;

        [Header("Timed Effects")]
        [SerializeField] List<TimedCharacterEffect> timedEffects;

        protected override void Awake()
        {
            base.Awake();
            
            GenerateEffectIDs();
        }

        private void GenerateEffectIDs()
        {
            for (int i = 0; i < instantEffects.Count; i++)
            {
                instantEffects[i].instantEffectID = i;
            }

            for (int i = 0; i < staticEffects.Count; i++)
            {
                staticEffects[i].staticEffectID = i;
            }

            for (int i = 0; i < timedEffects.Count; i++)
            {
                timedEffects[i].effectID = i;
            }
        }
    }
}
