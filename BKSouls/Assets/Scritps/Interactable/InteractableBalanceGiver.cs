using UnityEngine;

namespace BK
{
    public class InteractableBalanceGiver : Interactable
    {
        [SerializeField] private int balanceAmount = 10000;

        public override void Interact(PlayerManager player)
        {
            if (!IsEligibleInteractor(player))
                return;

            if (Inventory.WorldPlayerInventory.Instance != null)
                Inventory.WorldPlayerInventory.Instance.balance.Value += balanceAmount;

            base.Interact(player);
        }
    }
}
