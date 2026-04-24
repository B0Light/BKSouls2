using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class AIBossCharacterManager : AICharacterManager
    {
        [Header("Music")]
        [SerializeField] AudioClip bossIntroClip;
        [SerializeField] AudioClip bossBattleLoopClip;

        [Header("Status")]
        public NetworkVariable<bool> bossFightIsActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> hasBeenAwakened = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> hasBeenDefeated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        [SerializeField] string sleepAnimation;
        [SerializeField] string awakenAnimation;

        [Header("Phase Shift")]
        public float minimumHealthPercentageToShift = 50;
        [SerializeField] string phaseShiftAnimation = "Phase_Change_01";
        [SerializeField] CombatStanceState phase02CombatStanceState;

        [Header("States")]
        public BossSleepState sleepState;

        //  WHEN THIS A.I IS SPAWNED, CHECK OUR SAVE FILE (DICTIONARY)
        //  IF THE SAVE FILE DOES NOT CONTAIN A BOSS MONSTER WITH THIS I.D ADD IT
        //  IF IT IS PRESENT, CHECK IF THE BOSS HAS BEEN DEFEATED
        //  IF THE BOSS HAS BEEN DEFEATED, DISABLE THIS GAMEOBJECT
        //  IF THE BOSS HAS NOT BEEN DEFEATED, ALLOW THIS OBJECT TO CONTINUE TO BE ACTIVE

        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            bossFightIsActive.OnValueChanged += OnBossFightIsActiveChanged;
            OnBossFightIsActiveChanged(false, bossFightIsActive.Value); // SO IF YOU JOIN WHEN THE FIGHT IS ALREADY ACTIVE, YOU WILL GET A HP BAR

            if (IsOwner)
            {
                sleepState = Instantiate(sleepState);
                currentState = sleepState;
            }

            if (IsServer)
            {
                if (hasBeenDefeated.Value)
                    aiCharacterNetworkManager.isActive.Value = false;
            }
            
            if (!hasBeenAwakened.Value)
            {
                animator.Play(sleepAnimation);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            bossFightIsActive.OnValueChanged -= OnBossFightIsActiveChanged;
        }

        public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
        {
            GUIController.Instance.playerUIPopUpManager.SendBossDefeatedPopUp("GREAT FOE FELLED");

            if (IsOwner)
            {
                characterNetworkManager.currentHealth.Value = 0;
                isDead.Value = true;
                bossFightIsActive.Value = false;

                //  RESET ANY FLAGS HERE THAT NEED TO BE RESET
                //  NOTHING YET

                //  IF WE ARE NOT GROUNDED, PLAY AN AERIAL DEATH ANIMATION

                if (!manuallySelectDeathAnimation)
                    characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);

                hasBeenDefeated.Value = true;

                WorldSaveGameManager.Instance.SaveGame();
            }

            //  PLAY SOME DEATH SFX

            yield return new WaitForSeconds(5);

            //  AWARD PLAYERS WITH RUNES

            //  DISABLE CHARACTER
        }

        public void WakeBoss()
        {
            if (IsOwner)
            {
                if (!hasBeenAwakened.Value)
                    characterAnimatorManager.PlayTargetActionAnimation(awakenAnimation, true);

                bossFightIsActive.Value = true;
                hasBeenAwakened.Value = true;
                aiCharacterNetworkManager.isAwake.Value = true;
                currentState = idle;
            }
        }

        private void OnBossFightIsActiveChanged(bool oldStatus, bool newStatus)
        {
            if (bossFightIsActive.Value)
            {
                WorldSoundFXManager.Instance.PlayBossTrack(bossIntroClip, bossBattleLoopClip);

                GameObject bossHealthBar =
                Instantiate(GUIController.Instance.playerUIHudManager.bossHealthBarObject, GUIController.Instance.playerUIHudManager.bossHealthBarParent);

                UI_Boss_HP_Bar bossHPBar = bossHealthBar.GetComponentInChildren<UI_Boss_HP_Bar>();
                bossHPBar.EnableBossHPBar(this);
                GUIController.Instance.playerUIHudManager.currentBossHealthBar = bossHPBar;
            }
            else
            {
                WorldSoundFXManager.Instance.StopBossMusic();
            }
        }

        public void PhaseShift()
        {
            characterAnimatorManager.PlayTargetActionAnimation(phaseShiftAnimation, true);
            combatStance = Instantiate(phase02CombatStanceState);
            currentState = combatStance;
        }

        public override void ActivateCharacter(PlayerManager player)
        {
            if (hasBeenDefeated.Value)
            {
                DeactivateCharacter(player);
                return;
            }

            aiCharacterCombatManager.AddPlayerToPlayersWithinRange(player);

            if (player.IsLocalPlayer)
            {
                //  ENABLE RENDERERS (Optionally)
                //  RENDERERS CAN BE DISABLED FOR OTHER PLAYERS NOT NEAR THIS A.I, THIS WILL SAVE ON MEMORY
            }

            if (!NetworkManager.Singleton.IsHost)
                return;

            if (aiCharacterCombatManager.playersWithinActivationRange.Count > 0)
            {
                aiCharacterNetworkManager.isActive.Value = true;
            }
            else
            {
                aiCharacterNetworkManager.isActive.Value = false;
            }
        }
    }
}
