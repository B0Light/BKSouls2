using System.Collections;
using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class SiteOfGraceInteractable : Interactable
    {
        [Header("Site Of Grace Info")]
        public int siteOfGraceID;
        public NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Rest Cost")]
        [SerializeField] private int restCost = 100;

        [Header("VFX")]
        [SerializeField] GameObject activatedParticles;

        [Header("Interaction Text")]
        [SerializeField] string unactivatedInteractionText = "Restore Site Of Grace";
        [SerializeField] string activatedInteractionText = "Rest";

        [Header("Teleport Transform")]
        [SerializeField] Transform teleportTransform;

        private bool _hasPaidForRest = false;

        protected override void Start()
        {
            base.Start();

            if (isActivated.Value)
            {
                interactableText = activatedInteractionText;
            }
            else
            {
                interactableText = unactivatedInteractionText;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            //  IF WE JOIN WHEN THE STATUS HAS ALREADY CHANGED, WE FORCE THE ONCHANGE FUNCTION TO RUN HERE UPON JOINING
            if (!IsOwner)
                OnIsActivatedChanged(false, isActivated.Value);

            isActivated.OnValueChanged += OnIsActivatedChanged;

            WorldObjectManager.instance.AddSiteOfGraceToList(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            isActivated.OnValueChanged -= OnIsActivatedChanged;
        }

        private void RestoreSiteOfGrace(PlayerManager player)
        {
            isActivated.Value = true;

            player.playerAnimatorManager.PlayTargetActionAnimation("Activate_Site_Of_Grace_01", true);

            GUIController.Instance.playerUIPopUpManager.SendGraceRestoredPopUp("SITE OF GRACE RESTORED");

            StartCoroutine(WaitForAnimationThenOpenRestMenu(player));
        }

        private IEnumerator WaitForAnimationThenOpenRestMenu(PlayerManager player)
        {
            yield return new WaitForSeconds(2);
            interactableCollider.enabled = true;
            OpenRestUI(player);
        }

        private void RestAtSiteOfGrace(PlayerManager player)
        {
            interactableCollider.enabled = true;
            OpenRestUI(player);
        }

        private void OpenRestUI(PlayerManager player)
        {
            int cost = _hasPaidForRest ? 0 : restCost;
            GUIController.Instance.playerUISiteOfGraceManager.OpenRestMenu(cost, () => OnRestPaid(player), player);
        }

        private void OnRestPaid(PlayerManager player)
        {
            _hasPaidForRest = true;
            ApplyRestEffect(player);
        }

        private void ApplyRestEffect(PlayerManager player)
        {
            player.playerNetworkManager.currentHealth.Value = player.playerNetworkManager.maxHealth.Value;
            player.playerNetworkManager.currentStamina.Value = player.playerNetworkManager.maxStamina.Value;

            //  REFILL FLASKS
            //  인벤토리 포션(여분)은 건드리지 않고 기본 충전 횟수만 복원한다.
            if (player.IsOwner)
            {
                player.playerNetworkManager.remainingHealthFlasks.Value = 3;
                player.playerNetworkManager.remainingFocusPointsFlasks.Value = 1;
                GUIController.Instance.playerUIHudManager.SetQuickSlotItemQuickSlotIcon(
                    player.playerInventoryManager.currentQuickSlotItem);
            }

            WorldAIManager.instance.ResetAllCharacters();
        }

        private void OnIsActivatedChanged(bool oldStatus, bool newStatus)
        {
            if (isActivated.Value)
            {
                //  PLAY SOME FX HERE IF YOU'D LIKE OR ENABLE A LIGHT OR SOMETHING TO INDICATE THIS CHECK POINT IS ON
                activatedParticles.SetActive(true);
                interactableText = activatedInteractionText;
            }
            else
            {
                interactableText = unactivatedInteractionText;
            }
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            if (player.isPerformingAction)
                return;

            if (player.playerCombatManager.isUsingItem)
                return;

            if (!isActivated.Value)
            {
                RestoreSiteOfGrace(player);
            }
            else
            {
                RestAtSiteOfGrace(player);
            }
        }

        public void TeleportToSiteOfGrace()
        {
            //  THE PLAYER IS ONLY ABLE TO TELEPORT WHEN NOT IN A CO-OP GAME SO WE CAN GRAB THE LOCAL PLAYER FROM THE NETWORK MANAGER
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  ENABLE LOADING SCREEN
            GUIController.Instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            //  TELEPORT PLAYER
            player.transform.position = teleportTransform.position;

            //  DISABLE LOADING SCREEN
            GUIController.Instance.playerUILoadingScreenManager.DeactivateLoadingScreen(1);
        }
    }
}
