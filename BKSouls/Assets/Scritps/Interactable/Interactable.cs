using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class Interactable : NetworkBehaviour
    {
        [Header("UI")]
        public string interactableText; // Text prompt when player enters collider

        [Header("Interaction")]
        [SerializeField] protected Collider interactableCollider;
        [SerializeField] protected bool hostOnlyInteractable = true;

        protected virtual void Awake()
        {
            if (interactableCollider == null)
                interactableCollider = GetComponent<Collider>();
        }
        
        protected virtual void Start() { }

        public virtual void Interact(PlayerManager player)
        {
            Debug.Log("YOU HAVE INTERACTED!");

            if (!IsEligibleInteractor(player))
                return;

            // Disable further interaction locally for this player
            if (interactableCollider != null)
                interactableCollider.enabled = false;

            CleanupInteraction(player);

            // Save after interacting
            WorldSaveGameManager.Instance?.SaveGame();
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            if (!TryGetEligiblePlayer(other, out var player))
                return;

            player.playerInteractionManager?.AddInteractionToList(this);
        }

        public virtual void OnTriggerExit(Collider other)
        {
            if (!TryGetEligiblePlayer(other, out var player))
                return;

            CleanupInteraction(player);
        }

        // --------------------
        // Helpers
        // --------------------

        protected bool TryGetEligiblePlayer(Collider other, out PlayerManager player)
        {
            player = other.GetComponent<PlayerManager>();
            if (player == null) return false;

            if (hostOnlyInteractable && player.playerNetworkManager != null && !player.playerNetworkManager.IsHost)
                return false;

            if (!player.IsOwner)
                return false;

            return true;
        }

        protected bool IsEligibleInteractor(PlayerManager player)
        {
            if (player == null) return false;

            if (hostOnlyInteractable && player.playerNetworkManager != null && !player.playerNetworkManager.IsHost)
                return false;

            return player.IsOwner;
        }

        protected void CleanupInteraction(PlayerManager player)
        {
            player.playerInteractionManager?.RemoveInteractionFromList(this);
            GUIController.Instance?.playerUIPopUpManager?.CloseAllPopUpWindows();

            ResetInteraction();
        }
        
        public virtual void ResetInteraction()
        {
            if (interactableCollider != null)
                interactableCollider.enabled = true;
        }
        
        public virtual void SetToSpecificLevel(int level) {}
    }
}