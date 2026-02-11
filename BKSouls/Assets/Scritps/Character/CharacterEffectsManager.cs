using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class CharacterEffectsManager : MonoBehaviour
    {
        //  PROCESS INSTANT EFFECTS (TAKE DAMAGE, HEAL)

        //  PROCESS TIMED EFFECTS (POISON, BUILD UPS)

        //  PROCESS STATIC EFFECTS (ADDING/REMOVING BUFFS FROM TALISMANS ECT)

        protected CharacterManager character;

        [Header("Current Active FX")]
        public GameObject activeQuickSlotItemFX;
        public GameObject activeSpellWarmUpFX;
        public GameObject activeDrawnProjectileFX;

        [Header("VFX")]
        [SerializeField] GameObject bloodSplatterVFX;
        [SerializeField] GameObject criticalBloodSplatterVFX;

        [Header("Status Effect VFX")]
        [HideInInspector] public GameObject poisonedVFX;
        [HideInInspector] public GameObject frostBiteVFX;

        [Header("Static Effects")]
        public List<StaticCharacterEffect> staticEffects = new List<StaticCharacterEffect>();

        [Header("Timed Effects")]
        [SerializeField] protected float effectTickTimer = 0;
        [SerializeField] protected float defaultEffectTickTime = 1;
        public List<TimedCharacterEffect> timedEffects = new List<TimedCharacterEffect>();

        [Header("Frozen")]
        private Coroutine frozenCoroutine;

        [Header("Renderers")]
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        private MeshRenderer[] meshRenderers;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Update()
        {
            effectTickTimer -= Time.deltaTime;

            if (effectTickTimer <= 0)
            {
                effectTickTimer = defaultEffectTickTime;
                ProcessTimedEffects();
            }
        }

        public virtual void ProcessInstantEffect(InstantCharacterEffect effect)
        {
            effect.ProcessEffect(character);
        }

        public void PlayBloodSplatterVFX(Vector3 contactPoint)
        {
            //  IF WE MANUALLY HAVE PLACED A BLOOD SPLATTER VFX ON THIS MODEL, PLAY ITS VERSION
            if (bloodSplatterVFX != null)
            {
                GameObject bloodSplatter = Instantiate(bloodSplatterVFX, contactPoint, Quaternion.identity);
            }
            //  ELSE, USE THE GENERIC (DEFAULT VERSION) WE HAVE ELSEWHERE
            else
            {
                GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.bloodSplatterVFX, contactPoint, Quaternion.identity);
            }
        }

        public void PlayCriticalBloodSplatterVFX(Vector3 contactPoint)
        {
            //  IF WE MANUALLY HAVE PLACED A BLOOD SPLATTER VFX ON THIS MODEL, PLAY ITS VERSION
            if (bloodSplatterVFX != null)
            {
                GameObject bloodSplatter = Instantiate(criticalBloodSplatterVFX, contactPoint, Quaternion.identity);
            }
            //  ELSE, USE THE GENERIC (DEFAULT VERSION) WE HAVE ELSEWHERE
            else
            {
                GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.criticalBloodSplatterVFX, contactPoint, Quaternion.identity);
            }
        }

        public virtual void AddBuildUps(BuildUp buildUpType, float amount)
        {
            if (!character.IsOwner)
                return;

            switch (buildUpType)
            {
                case BuildUp.Poison:
                    //  IF CHARACTER IS ALREADY POISONED, RETURN
                    //  TO DO, PASS THE VALUE THROUGH THE CHARACTERS POISON RESISTANCE FIRST (SIMILAR TO ARMOR)
                    character.characterNetworkManager.poisonBuildUp.Value += amount;
                    break;
                case BuildUp.Bleed:
                    //  TO DO, PASS THE VALUE THROUGH THE CHARACTERS BLEED RESISTANCE FIRST (SIMILAR TO ARMOR)
                    character.characterNetworkManager.bleedBuildUp.Value += amount;
                    break;
                case BuildUp.Frost:
                    //  TO DO, PASS THE VALUE THROUGH THE CHARACTERS FROST RESISTANCE FIRST (SIMILAR TO ARMOR)
                    character.characterNetworkManager.frostBiteBuildUp.Value += amount;
                    break;
                default:
                    break;
            }
        }

        //  STATIC EFFECTS
        public void AddStaticEffect(StaticCharacterEffect effect)
        {
            //  IF YOU WANT TO SYNC EFFECTS ACROSS NETWORK, IF YOU ARE THE OWNER LAUNCH A SERVER RPC HERE TO PROCESS THE EFFECT ON ALL OTHER CLIENTS

            // 1. ADD A STATIC EFFECT TO THE CHARACTER
            staticEffects.Add(effect);

            // 2. PROCESS ITS EFFECT
            effect.ProcessStaticEffect(character);

            // 3. CHECK FOR NULL ENTRIES IN YOUR LIST AND REMOVE THEM
            for (int i = staticEffects.Count - 1; i > -1; i--)
            {
                if (staticEffects[i] == null)
                    staticEffects.RemoveAt(i);
            }
        }

        public void RemoveStaticEffect(int effectID)
        {
            //  IF YOU WANT TO SYNC EFFECTS ACROSS NETWORK, IF YOU ARE THE OWNER LAUNCH A SERVER RPC HERE TO PROCESS THE EFFECT ON ALL OTHER CLIENTS

            StaticCharacterEffect effect;

            for (int i = 0; i < staticEffects.Count; i++)
            {
                if (staticEffects[i] != null)
                {
                    if (staticEffects[i].staticEffectID == effectID)
                    {
                        effect = staticEffects[i];
                        // 1. REMOVE STATIC EFFECT FROM CHARACTER
                        effect.RemoveStaticEffect(character);
                        // 2. REMOVE STATIC EFFECT FROM LIST
                        staticEffects.Remove(effect);
                    }
                }
            }

            // 3. CHECK FOR NULL ENTRIES IN YOUR LIST AND REMOVE THEM
            for (int i = staticEffects.Count - 1; i > -1; i--)
            {
                if (staticEffects[i] == null)
                    staticEffects.RemoveAt(i);
            }
        }

        //  TIMED EFFECTS

        //  PROCESSES ALL CURRENT TIMED EFFECTS
        public void ProcessTimedEffects()
        {
            for (int i = 0; i < timedEffects.Count; i++)
            {
                if (timedEffects[i] == null)
                    continue;

                timedEffects[i].ProcessEffect(character);
            }
        }

        //  ADDS A NEW EFFECT
        public void AddTimedEffect(TimedCharacterEffect effect)
        {
            bool effectIsAlreadyOnCharacter = false;

            //  IF WE ALREADY HAVE THE EFFECT, JUST RESTART ITS TIMER AGAIN INSTEAD OF ADDING A DUPLICATE
            for (int i = 0; i < timedEffects.Count; i++)
            {
                if (timedEffects[i] == null)
                    continue;

                if (timedEffects[i].effectID == effect.effectID)
                {
                    effectIsAlreadyOnCharacter = true;
                    timedEffects[i].timeRemainingOnEffect = timedEffects[i].defaultLengthOfEffect;
                }
            }

            if (!effectIsAlreadyOnCharacter)
            {
                timedEffects.Add(effect);
                effect.timeRemainingOnEffect = effect.defaultLengthOfEffect;

                //  PROCESS THE FIRST "TICK" INSTANTLY
                effect.ProcessEffect(character);
            }
        }

        //  REMOVES AN EFFECT
        public void RemoveTimedEffect(int effectID)
        {
            TimedCharacterEffect effect;

            //  FIND AND REMOVE
            for (int i = 0; i < timedEffects.Count; i++)
            {
                if (timedEffects[i] == null)
                    continue;

                if (timedEffects[i].effectID == effectID)
                {
                    effect = timedEffects[i];
                    effect.RemoveEffect(character);
                    timedEffects.Remove(effect);
                }
            }

            //  REMOVE NULL ENTRIES FROM LIST
            for (int i = 0; i < timedEffects.Count; i++)
            {
                if (timedEffects[i] == null)
                    timedEffects.RemoveAt(i);
            }
        }

        //  CHECKS IF WE ALREADY HAVE A SPECIFIC EFFECT (AND GETS IT)
        public TimedCharacterEffect CheckForTimedEffect(int effectID)
        {
            TimedCharacterEffect timedEffect = null;

            for (int i = 0; i < timedEffects.Count; i++)
            {
                if (timedEffects[i].effectID == effectID)
                {
                    timedEffect = timedEffects[i];
                    break;
                }
            }

            return timedEffect;
        }
        
        public void ProcessEffectDamage(int effectDamage)
        {
            //  IF YOU ARE SYNCING YOUR EFFECTS ON SERVER RPC CALLS REMEMBER TO CHECK FOR OWNER BEFORE MODIFYING A NETWORK VARAIBLE
            if (!character.IsOwner)
                return;

            if (character.isDead.Value)
                return;

            character.characterNetworkManager.currentHealth.Value -= effectDamage;

            if (character.characterNetworkManager.currentHealth.Value >= 1)
                return;

            //  IF WE ARE PLAYING A RIPOSTE OR BACK STAB ANIMATION, DO NOT BREAK IT
            if (!character.characterNetworkManager.isBeingCriticallyDamaged.Value)
                character.characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);

            character.characterNetworkManager.isPoisoned.Value = false;
            character.characterNetworkManager.isBleeding.Value = false;
            character.characterNetworkManager.isFrostBitten.Value = false;
            character.characterNetworkManager.isFrozen.Value = false;
            character.isDead.Value = true;
        }

        //  FROZEN
        public void PlayFrozenFX()
        {
            //  IF YOU ARE USING ANIMATION IK, TEMPORARILY DISABLE IT HERE

            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            if (frozenCoroutine != null)
                StopCoroutine(frozenCoroutine);

            frozenCoroutine = StartCoroutine(ActivateFrozenVFXCoroutine(WorldUtilityManager.Instance.GetFrozenMaterial()));
        }

        private IEnumerator ActivateFrozenVFXCoroutine(Material frozenMaterial)
        {
            //  ALL CHARACTER SKIN MESH RENDERER MATERIALS
            List<Material> originalSkinMeshMaterials = new List<Material>();

            //  ANY MARTERIALS OF OBJECTS THE CHARACTER HAS ON THEIR MODEL (SUCH AS A WEAPON)
            List<Material> originalMeshMaterials = new List<Material>();

            //  SAVE WHAT ARE CHARACTER'S STATUS WAS BEFORE WE WERE FROZEN
            bool rotationStatusOnFrozen = character.characterLocomotionManager.canRotate;
            bool canMoveStatusOnFrozen = character.characterLocomotionManager.canMove;
            bool isPerformingActionStatusOnFrozen = character.isPerformingAction;

            //  FREEZE THEIR ABILITY TO MOVE OR PERFORM AN ACTION
            character.characterLocomotionManager.canRotate = false;
            character.characterLocomotionManager.canMove = false;
            character.isPerformingAction = true;

            //  CHANGE ALL CHARACTER MATERIALS TO FROZEN MATERIAL
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null)
                    continue;

                //  INSTANTIATE A COPY IF ANY PROPERTIES ON YOUR MATERIAL CHANGE DURING RUNTIME
                originalSkinMeshMaterials.Add(Instantiate(skinnedMeshRenderers[i].material));
                skinnedMeshRenderers[i].material = Instantiate(frozenMaterial);
            }

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] == null)
                    continue;

                originalMeshMaterials.Add(Instantiate(meshRenderers[i].material));
                meshRenderers[i].material = Instantiate(frozenMaterial);
            }

            while (character.characterNetworkManager.isFrozen.Value)
            {
                yield return null;
            }

            //  UPON BEING UNFROZEN, CHANGE ALL MATERIALS BACK TO THEIR ORIGINAL MATERIAL
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                for (int j = 0; j < originalSkinMeshMaterials.Count; j++)
                {
                    skinnedMeshRenderers[i].material = originalSkinMeshMaterials[j];
                }
            }

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                for (int j = 0; j < originalMeshMaterials.Count; j++)
                {
                    meshRenderers[i].material = originalMeshMaterials[j];
                }
            }

            character.characterLocomotionManager.canRotate = rotationStatusOnFrozen;
            character.characterLocomotionManager.canMove = canMoveStatusOnFrozen;
            character.isPerformingAction = isPerformingActionStatusOnFrozen;


            //  IS THERE AN ALTERNATIVE TO CHANGING THE MATERIALS?

            //  YES! YOU COULD MAKE A SHADER WITH A "FROZEN" PROPERTY, WHICH COULD ADD A LAYER OF ICE OVER THE STANDARD MATERIAL USING THE SHADER
            //  THEN, INSTEAD OF CHANGING MATERIALS, SIMPLY SET THE FROZEN VARAIBLE VALUE TO THE DESIRED SETTING, AND CHANGE IT BACK TO 0 WHEN UNFROZEN
        }
    }
}
