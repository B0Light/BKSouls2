using BK;
using UnityEngine;

namespace BK.Inventory
{

    
    public class InteractableForge : Interactable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string openAnimationTrigger = "Open";
        [SerializeField] private string closeAnimationTrigger = "Close";

        private bool _isOpen = false;

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);
            WorldSaveGameManager.Instance?.SaveGame();

            _isOpen = true;
            if (animator != null) animator.SetTrigger(openAnimationTrigger);
            GUIController.Instance.OpenForge(this);
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();

            if (_isOpen)
            {
                _isOpen = false;
                if (animator != null) animator.SetTrigger(closeAnimationTrigger);
            }
        }
    }
}
