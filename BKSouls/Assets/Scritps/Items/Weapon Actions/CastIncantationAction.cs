using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Incantation Action")]
    public class CastIncantationAction : WeaponItemAction
    {
        [Header("Spell")]
        public SpellItem spell;

        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
                return;

            if (spell == null)
                return;

            if (spell.SpellClass != SpellClass.Incantation)
                return;

            //  애니메이션 콜백(InstantiateSpellWarmUpFX 등)이 참조할 수 있도록 현재 스펠로 설정
            playerPerformingAction.playerInventoryManager.currentSpell = spell;

            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isAttacking.Value = true;

            CastIncantation(playerPerformingAction, weaponPerformingAction);
        }

        private void CastIncantation(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            spell.AttemptToCastSpell(playerPerformingAction);
        }
    }
}
