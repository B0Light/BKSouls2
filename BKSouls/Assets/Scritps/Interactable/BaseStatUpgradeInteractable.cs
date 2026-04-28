using UnityEngine;

namespace BK
{
    public class BaseStatUpgradeInteractable : Interactable
    {
        [Header("Visual Effect")]
        [SerializeField] private GameObject vfxObject;

        protected override void Awake()
        {
            base.Awake();

            interactableText = "Base Stat Upgrade";
            hostOnlyInteractable = false;

            if (vfxObject != null)
                vfxObject.SetActive(false);
        }

        public override void Interact(PlayerManager player)
        {
            if (player.isPerformingAction || player.playerCombatManager.isUsingItem)
                return;

            GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
            GUIController.Instance.playerUIBaseStatUpgradeManager.OpenMenu();
        }

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (vfxObject != null)
                vfxObject.SetActive(true);
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            if (vfxObject != null)
                vfxObject.SetActive(false);
        }
    }
}
