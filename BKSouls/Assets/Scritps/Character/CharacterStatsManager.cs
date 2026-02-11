using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class CharacterStatsManager : MonoBehaviour
    {
        CharacterManager character;

        [Header("Runes")]
        public int runesDroppedOnDeath = 50;

        [Header("Stamina Regeneration")]
        [SerializeField] float baseStaminaRegenerationAmount = 2;
        private float staminaRegenerationAmount = 0;
        private float staminaTickTimer = 0;
        private float staminaRegenerationTimer = 0;
        [SerializeField] float staminaRegenerationDelay = 2;
        
        [Header("Blocking Absorptions")]
        public float blockingPhysicalAbsorption;
        public float blockingFireAbsorption;
        public float blockingMagicAbsorption;
        public float blockingLightningAbsorption;
        public float blockingHolyAbsorption;
        public float blockingStability;

        [Header("Armor Absorption")]
        public float armorPhysicalDamageAbsorption;
        public float armorMagicDamageAbsorption;
        public float armorFireDamageAbsorption;
        public float armorHolyDamageAbsorption;
        public float armorLightningDamageAbsorption;

        [Header("Armor Resistances")]
        public float armorImmunity;      // RESISTANCE TO ROT AND POISON
        public float armorRobustness;    // RESISTANCE TO BLEED AND FROST
        public float armorFocus;         // RESISTANCE TO MADNESS AND SLEEP
        public float armorVitality;      // RESISTANCE TO DEATH CURSE

        [Header("Poise")]
        public float totalPoiseDamage;              // How much poise damage we have taken
        public float offensivePoiseBonus;           // The poise bonus gained from using weapons (heavy weapons have a much larger bonus)
        public float basePoiseDefense;              // The poise bonus gained from armor/talismans ect
        public float defaultPoiseResetTime = 8;     // The time it takes for poise damage to reset (must not be hit in the time or it will reset)
        public float poiseResetTimer = 0;           // The current timer for poise reset

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            HandlePoiseResetTimer();
        }

        public int CalculateHealthBasedOnVitalityLevel(int vitality)
        {
            float health = 0;

            //  CREATE AN EQUATION FOR HOW YOU WANT YOUR STAMINA TO BE CALCULATED

            health = vitality * 15;

            return Mathf.RoundToInt(health);
        }

        public int CalculateStaminaBasedOnEnduranceLevel(int endurance)
        {
            float stamina = 0;

            //  CREATE AN EQUATION FOR HOW YOU WANT YOUR STAMINA TO BE CALCULATED

            stamina = endurance * 10;

            return Mathf.RoundToInt(stamina);
        }

        public int CalculateFocusPointsBasedOnMindLevel(int mind)
        {
            int focusPoints = 0;

            //  CREATE AN EQUATION FOR HOW YOU WANT YOUR STAMINA TO BE CALCULATED

            focusPoints = mind * 10;

            return Mathf.RoundToInt(focusPoints);
        }

        public int CalculateCharacterLevelBasedOnAttributes(bool calculateProjectedLevel = false)
        {
            //  IN ELDEN RING & SOULS YOU GET 10 X FREE LEVELS PER ATTRIBUTE BEFORE IT STARTS TO ADD ONTO YOUR PLAYER LEVEL
            //  FOR EX
            //  WE HAVE VIGOR, MIND, ENDURANCE, STRENGTH, DEXTERITY, INTELLIGENCE AND FAITH. (7 ATTRIBUTES) THIS EQUATES TO 70 LEVELS BEFORE YOU COULD PAST LEVEL 1


            if (calculateProjectedLevel)
            {
                int totalProjectedAttributes = 
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.vigorSlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.mindSlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.enduranceSlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.strengthSlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.dexteritySlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.intelligenceSlider.value) +
                    Mathf.RoundToInt(GUIController.Instance.playerUILevelUpManager.faithSlider.value);

                int projectedCharacterLevel = totalProjectedAttributes - 70 + 1;

                if (projectedCharacterLevel < 1)
                    projectedCharacterLevel = 1;

                return projectedCharacterLevel;
            }

            int totalAttributes = character.characterNetworkManager.vigor.Value +
                character.characterNetworkManager.mind.Value +
                character.characterNetworkManager.endurance.Value +
                character.characterNetworkManager.strength.Value +
                character.characterNetworkManager.dexterity.Value +
                character.characterNetworkManager.intelligence.Value +
                character.characterNetworkManager.faith.Value;

            int characterLevel = totalAttributes - 70 + 1;

            if (characterLevel < 1)
                characterLevel = 1;

            return characterLevel;
        }

        public int CalculateBuildUpCapacityBasedOnVitalityLevel(int vitality)
        {
            float capacity = 0;

            //  CREATE AN EQUATION FOR HOW YOU WANT YOUR STAMINA TO BE CALCULATED

            capacity = vitality * 3.25f;

            return Mathf.RoundToInt(capacity);
        }

        public virtual void RegenerateStamina()
        {
            //  ONLY OWNERS CAN EDIT THEIR NETWORK VARAIBLES
            if (!character.IsOwner)
                return;

            //  WE DO NOT WANT TO REGENERATE STAMINA IF WE ARE USING IT
            if (character.characterNetworkManager.isSprinting.Value)
                return;

            if (character.isPerformingAction)
                return;

            if (character.characterNetworkManager.currentStamina.Value >= character.characterNetworkManager.maxStamina.Value)
                return;

            staminaRegenerationAmount = baseStaminaRegenerationAmount + (baseStaminaRegenerationAmount * (character.characterNetworkManager.staminaRegenerationModifier.Value / 100));
            staminaTickTimer += Time.deltaTime;

            Debug.Log("STAMINA REGENERATION AMOUNT: " + staminaRegenerationAmount);

            //  IF WE ARE BLOCKING, RECOVER STAMINA SLOWER THAN USUAL
            if (character.characterNetworkManager.isBlocking.Value)
                staminaRegenerationAmount *= 0.2f;

            staminaRegenerationTimer += Time.deltaTime;
            
            if (staminaRegenerationTimer >= staminaRegenerationDelay)
            {
                if (character.characterNetworkManager.currentStamina.Value < character.characterNetworkManager.maxStamina.Value)
                {
                    staminaTickTimer += Time.deltaTime;

                    if (staminaTickTimer >= 0.1)
                    {
                        staminaTickTimer = 0;
                        character.characterNetworkManager.currentStamina.Value += staminaRegenerationAmount;
                    }
                }
            }
        }
        
        public virtual void ResetStaminaRegenTimer(float previousStaminaAmount, float currentStaminaAmount)
        {
            //  WE ONLY WANT TO RESET THE REGENERATION IF THE ACTION USED STAMINA
            //  WE DONT WANT TO RESET THE REGENERATION IF WE ARE ALREADY REGENERATING STAMINA
            if (currentStaminaAmount < previousStaminaAmount)
            {
                staminaRegenerationTimer = 0;
            }
        }

        protected virtual void HandlePoiseResetTimer()
        {
            if (poiseResetTimer > 0)
            {
                poiseResetTimer -= Time.deltaTime;
            }
            else
            {
                totalPoiseDamage = 0;
            }
        }

        public virtual void DegradeBuildUps(BuildUp buildUp, int amount, BuildUpEffect effect)
        {
            switch (buildUp)
            {
                case BuildUp.Poison:
                    character.characterNetworkManager.poisonBuildUp.Value += amount;
                    effect.buildUpRemaining = character.characterNetworkManager.poisonBuildUp.Value;
                    break;
                case BuildUp.Bleed:
                    character.characterNetworkManager.bleedBuildUp.Value += amount;
                    effect.buildUpRemaining = character.characterNetworkManager.bleedBuildUp.Value;
                    break;
                case BuildUp.Frost:
                    character.characterNetworkManager.frostBiteBuildUp.Value += amount;
                    effect.buildUpRemaining = character.characterNetworkManager.frostBiteBuildUp.Value;
                    break;
                default:
                    break;
            }
        }
    }
}
