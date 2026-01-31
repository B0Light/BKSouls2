using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "A.I/States/Attack")]
    public class AttackState : AIState
    {
        [Header("Current Attack")]
        [HideInInspector] public AICharacterAttackAction currentAttack;
        [HideInInspector] public bool willPerformCombo = false;

        [Header("State Flags")]
        protected bool hasPerformedAttack = false;
        protected bool hasPerformedCombo = false;

        [Header("Pivot After Attack")]
        [SerializeField] protected bool pivotAfterAttack = false;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);

            if (aiCharacter.aiCharacterCombatManager.currentTarget.isDead.Value)
                return SwitchState(aiCharacter, aiCharacter.idle);

            aiCharacter.aiCharacterCombatManager.RotateTowardsTargetWhilstAttacking(aiCharacter);

            aiCharacter.characterAnimatorManager.UpdateAnimatorMovementParameters(0, 0, false);

            //  PERFORM A COMBO
            PerformCombo(aiCharacter);

            if (aiCharacter.isPerformingAction)
                return this;

            if (!hasPerformedAttack)
            {
                //  IF WE ARE STILL RECOVERING FROM AN ACTION, WAIT BEFORE PERFORMING ANOTHER
                if (aiCharacter.aiCharacterCombatManager.actionRecoveryTimer > 0)
                    return this;

                PerformAttack(aiCharacter);

                //  RETURN TO THE TOP, SO IF WE HAVE A COMBO WE PROCESS THAT WHEN WE ARE ABLE
                return this;
            }

            if (pivotAfterAttack)
                aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);

            return SwitchState(aiCharacter, aiCharacter.combatStance);
        }

        protected void PerformAttack(AICharacterManager aiCharacter)
        {
            hasPerformedAttack = true;
            currentAttack.AttemptToPerformAction(aiCharacter);
            aiCharacter.aiCharacterCombatManager.actionRecoveryTimer = currentAttack.actionRecoveryTime;
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasPerformedAttack = false;
            hasPerformedCombo = false;
            willPerformCombo = false;
        }

        protected virtual void PerformCombo(AICharacterManager aiCharacter)
        {
            bool canPerformTheCombo = false;

            if (!willPerformCombo)
                return;

            if (hasPerformedCombo)
                return;

            if (currentAttack.comboAction == null)
                return;

            //  IF WE DONT NEED TO HIT OUR CURRENT TARGET, WE WILL PERFORM THE COMBO ANYWAY
            if (aiCharacter.aiCharacterCombatManager.canPerformCombo
                && !aiCharacter.combatStance.onlyPerformComboIfInitialAttackHits)
                canPerformTheCombo = true;

            //  IF WE DO NEED TO HIT THE TARGET, AND WE HAVE HIT THE TARGET, PERFORM THE COMBO
            if (aiCharacter.aiCharacterCombatManager.canPerformCombo
                && aiCharacter.combatStance.onlyPerformComboIfInitialAttackHits
                && aiCharacter.aiCharacterCombatManager.hasHitTargetDuringCombo)
                canPerformTheCombo = true;

            if (canPerformTheCombo)
            {
                hasPerformedCombo = true;
                currentAttack.comboAction.AttemptToPerformAction(aiCharacter);
            }

        }
    }
}
