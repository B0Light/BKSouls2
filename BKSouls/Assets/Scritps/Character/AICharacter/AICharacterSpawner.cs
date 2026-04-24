using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class AICharacterSpawner : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private GameObject characterGameObject;
        [SerializeField] private GameObject instantiatedGameObject;
        private AICharacterManager aiCharacter;

        [Header("Patrol")]
        [SerializeField] private bool hasPatrolPath = false;
        [SerializeField] private int patrolPathID = 0;

        [Header("Sleep")]
        [SerializeField] private bool isSleeping = false;

        [Header("Stats")]
        [SerializeField] private AICharacterStatsSO statsSO;
        [SerializeField] private bool manuallySetStats = true;
        [SerializeField] private int stamina = 150;
        [SerializeField] private int health = 400;

        public AICharacterManager SpawnedAICharacter => aiCharacter;
        public GameObject SpawnedGameObject => instantiatedGameObject;

        public AICharacterManager AttemptToSpawnCharacter()
        {
            if (characterGameObject == null)
            {
                Debug.LogWarning("[AICharacterSpawner] characterGameObject is null.");
                return null;
            }

            instantiatedGameObject = Instantiate(characterGameObject, transform.position, transform.rotation);

            NetworkObject networkObject = instantiatedGameObject.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[AICharacterSpawner] Spawned character has no NetworkObject.");
                Destroy(instantiatedGameObject);
                return null;
            }

            networkObject.Spawn();

            aiCharacter = instantiatedGameObject.GetComponent<AICharacterManager>();
            if (aiCharacter == null)
            {
                Debug.LogError("[AICharacterSpawner] Spawned character has no AICharacterManager.");
                networkObject.Despawn(true);
                return null;
            }

            if (WorldAIManager.instance != null)
            {
                WorldAIManager.instance.AddCharacterToSpawnedCharactersList(aiCharacter);

                if (hasPatrolPath)
                    aiCharacter.idle.aiPatrolPath = WorldAIManager.instance.GetAIPatrolPathByID(patrolPathID);
            }

            if (isSleeping)
                aiCharacter.aiCharacterNetworkManager.isAwake.Value = false;

            if (statsSO != null)
            {
                aiCharacter.aiCharacterNetworkManager.maxHealth.Value = statsSO.maxHealth;
                aiCharacter.aiCharacterNetworkManager.currentHealth.Value = statsSO.maxHealth;
                aiCharacter.aiCharacterNetworkManager.maxStamina.Value = statsSO.maxStamina;
                aiCharacter.aiCharacterNetworkManager.currentStamina.Value = statsSO.maxStamina;
                aiCharacter.characterStatsManager.runesDroppedOnDeath = statsSO.runesDroppedOnDeath;
                aiCharacter.characterStatsManager.armorPhysicalDamageAbsorption = statsSO.armorPhysicalDamageAbsorption;
                aiCharacter.characterStatsManager.armorMagicDamageAbsorption = statsSO.armorMagicDamageAbsorption;
                aiCharacter.characterStatsManager.armorFireDamageAbsorption = statsSO.armorFireDamageAbsorption;
                aiCharacter.characterStatsManager.armorHolyDamageAbsorption = statsSO.armorHolyDamageAbsorption;
                aiCharacter.characterStatsManager.armorLightningDamageAbsorption = statsSO.armorLightningDamageAbsorption;
                aiCharacter.characterStatsManager.armorImmunity = statsSO.armorImmunity;
                aiCharacter.characterStatsManager.armorRobustness = statsSO.armorRobustness;
                aiCharacter.characterStatsManager.armorFocus = statsSO.armorFocus;
                aiCharacter.characterStatsManager.armorVitality = statsSO.armorVitality;
                aiCharacter.characterStatsManager.basePoiseDefense = statsSO.basePoiseDefense;
            }
            else if (manuallySetStats)
            {
                aiCharacter.aiCharacterNetworkManager.maxHealth.Value = health;
                aiCharacter.aiCharacterNetworkManager.currentHealth.Value = health;
                aiCharacter.aiCharacterNetworkManager.maxStamina.Value = stamina;
                aiCharacter.aiCharacterNetworkManager.currentStamina.Value = stamina;
            }

            aiCharacter.aiCharacterNetworkManager.isActive.Value = false;

            return aiCharacter;
        }

        public void ResetCharacter()
        {
            if (instantiatedGameObject == null || aiCharacter == null)
                return;

            instantiatedGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            aiCharacter.aiCharacterNetworkManager.currentHealth.Value = aiCharacter.aiCharacterNetworkManager.maxHealth.Value;
            aiCharacter.aiCharacterCombatManager.SetTarget(null);

            if (aiCharacter.isDead.Value)
            {
                aiCharacter.isDead.Value = false;
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Empty", false, false, true, true, true, true);
                aiCharacter.currentState.SwitchState(aiCharacter, aiCharacter.idle);
            }

            aiCharacter.characterUIManager.ResetCharacterHPBar();

            if (aiCharacter is AIBossCharacterManager boss)
            {
                boss.aiCharacterNetworkManager.isAwake.Value = false;
                boss.sleepState.hasBeenAwakened = boss.hasBeenAwakened.Value;
                boss.currentState = boss.currentState.SwitchState(boss, boss.sleepState);
            }
        }
    }
}