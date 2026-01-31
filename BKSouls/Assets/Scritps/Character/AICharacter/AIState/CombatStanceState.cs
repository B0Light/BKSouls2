using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BK
{
    [CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
    public class CombatStanceState : AIState
    {
        [Header("Attacks")]
        public List<AICharacterAttackAction> aiCharacterAttacks;    //  A list of all possible attacks this character can do
        [SerializeField] protected List<AICharacterAttackAction> potentialAttacks;     //  All attacks possible in this situation (based on angle, distance ect)
        [SerializeField] private AICharacterAttackAction choosenAttack;
        [SerializeField] private AICharacterAttackAction previousAttack;
        protected bool hasAttack = false;

        [Header("Combo")]
        [SerializeField] protected bool canPerformCombo = false;    // If the character can perform a combo attack, after the initial attack
        [SerializeField] protected int percentageOfTimeWillPerformCombo = 25;   // The chance (in percent) of the character to perform a combo on the next attack
        [SerializeField] public bool onlyPerformComboIfInitialAttackHits = false;
        protected bool hasRolledForComboChance = false;      // If we have already rolled for the chance during this state

        [Header("Engagement Distance")]
        [SerializeField] public float maximumEngagementDistance = 5; //  The distance we have to be away from the target before we enter the pursue target state

        [Header("Circling")]
        [SerializeField] bool willCircleTarget = false;
        private bool hasChoosenCirclePath = false;
        private float strafeMoveAmount;

        [Header("Blocking")]
        [SerializeField] bool canBlock = false;
        [SerializeField] int percentageOfTimeWillBlock = 75;
        private bool hasRolledForBlockChance = false;
        private bool willBlockDuringThisCombatRotation = false;

        [Header("Evasion")]
        [SerializeField] bool canEvade = false;
        [SerializeField] int percentageOfTimeWillEvade = 75;
        private bool hasEvaded = false;
        private bool hasRolledForEvasionChance = false;
        private bool willEvadeDuringThisCombatRotation = false;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return this;

            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;

            if (aiCharacter.aiCharacterCombatManager.currentTarget.isDead.Value)
                aiCharacter.aiCharacterCombatManager.SetTarget(null);

            //  IF YOU WANT THE AI CHARACTER TO FACE AND TURN TOWARDS ITS TARGET WHEN ITS OUTSIDE IT'S FOV INCLUDE THIS
            if (aiCharacter.aiCharacterCombatManager.enablePivot)
            {
                if (!aiCharacter.aiCharacterNetworkManager.isMoving.Value)
                {
                    if (aiCharacter.aiCharacterCombatManager.viewableAngle < -30 || aiCharacter.aiCharacterCombatManager.viewableAngle > 30)
                        aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);
                }
            }

            aiCharacter.aiCharacterCombatManager.RotateTowardsAgent(aiCharacter);

            //  IF OUR TARGET IS NO LONGER PRESENT, SWITCH BACK TO IDLE
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);

            if (willCircleTarget)
                SetCirclePath(aiCharacter);

            //  ROLL FOR BLOCK CHANCE
            if (canBlock && !hasRolledForBlockChance)
            {
                hasRolledForBlockChance = true;
                willBlockDuringThisCombatRotation = RollForOutcomeChance(percentageOfTimeWillBlock);
            }

            //  ROLL FOR EVASION CHANCE
            if (canEvade && !hasRolledForEvasionChance)
            {
                hasRolledForEvasionChance = true;
                willEvadeDuringThisCombatRotation = RollForOutcomeChance(percentageOfTimeWillEvade);
            }

            //  ROLL FOR COMBO CHANCE
            if (canPerformCombo && !hasRolledForComboChance)
            {
                hasRolledForComboChance = true;
                aiCharacter.attack.willPerformCombo = RollForOutcomeChance(percentageOfTimeWillPerformCombo);
            }

            if (willBlockDuringThisCombatRotation)
                aiCharacter.aiCharacterNetworkManager.isBlocking.Value = true;

            if (willEvadeDuringThisCombatRotation && aiCharacter.aiCharacterCombatManager.currentTarget.characterNetworkManager.isAttacking.Value && !hasEvaded)
            {
                hasEvaded = true;
                aiCharacter.aiCharacterCombatManager.PerformEvasion();
            }

            //  IF WE DO NOT HAVE AN ATTACK, GET ONE
            if (!hasAttack)
            {
                GetNewAttack(aiCharacter);
            }
            else
            {
                aiCharacter.attack.currentAttack = choosenAttack;
                //  ROLL FOR COMBO CHANCE
                return SwitchState(aiCharacter, aiCharacter.attack);
            }

            //  IF WE ARE OUTSIDE OF THE COMBAT ENGAGEMENT DISTANCE, SWITCH TO PURSUE TARGET STATE
            if (aiCharacter.aiCharacterCombatManager.distanceFromTarget > maximumEngagementDistance)
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);

            NavMeshPath path = new NavMeshPath();
            aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aiCharacterCombatManager.currentTarget.transform.position, path);
            aiCharacter.navMeshAgent.SetPath(path);

            return this;
        }

        protected virtual void GetNewAttack(AICharacterManager aiCharacter)
        {
            potentialAttacks = new List<AICharacterAttackAction>();

            foreach (var potentialAttack in aiCharacterAttacks)
            {
                //  IF WE ARE TOO CLOSE FOR THIS ATTACK, CHECK THE NEXT
                if (potentialAttack.minimumAttackDistance > aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                    continue;

                //  IF WE ARE TOO FAR FOR THIS ATTACK, CHECK THE NEXT
                if (potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                    continue;

                //  IF THE TARGET IS OUTSIDE MINIMUM FIELD OF VIEW FOR THIS ATTACK, CHECK THE NEXT
                if (potentialAttack.minimumAttackAngle > aiCharacter.aiCharacterCombatManager.viewableAngle)
                    continue;

                //  IF THE TARGET IS OUTSIDE MAXIMUM FIELD OF VIEW FOR THIS ATTACK, CHECK THE NEXT
                if (potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.viewableAngle)
                    continue;

                potentialAttacks.Add(potentialAttack);
            }

            if (potentialAttacks.Count <= 0)
                return;

            var totalWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                totalWeight += attack.attackWeight;
            }

            var randomWeightValue = Random.Range(1, totalWeight + 1);
            var processedWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                processedWeight += attack.attackWeight;

                if (randomWeightValue <= processedWeight)
                {
                    choosenAttack = attack;
                    previousAttack = choosenAttack;
                    hasAttack = true;
                    return;
                }
            }
        }

        protected virtual bool RollForOutcomeChance(int outcomeChance)
        {
            bool outcomeWillBePerformed = false;

            int randomPercentage = Random.Range(0, 100);

            if (randomPercentage < outcomeChance)
                outcomeWillBePerformed = true;

            return outcomeWillBePerformed;
        }

        protected virtual void SetCirclePath(AICharacterManager aiCharacter)
        {
            if (Physics.CheckSphere(aiCharacter.aiCharacterCombatManager.lockOnTransform.position, aiCharacter.characterController.radius + 0.25f, WorldUtilityManager.Instance.GetEnviroLayers()))
            {
                //  STOP STRAFING/CIRCLING BECAUSE WE'VE HIT SOMETHING, INSTEAD PATH TOWARDS ENEMY (WE USE ABS INCASE ITS NEGATIVE, TO MAKE IT POSITIVE)
                //  THIS WILL MAKE OUR CHARACTER FOLLOW THE NAVMESH AGENT AND PATH TOWARDS THE TARGET
                Debug.Log("WE ARE COLLIDING WITH SOMETHING, ENDING STRAFE");
                aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, Mathf.Abs(strafeMoveAmount));
                return;
            }

            //  STRAFE
            Debug.Log("STRAFING");
            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(strafeMoveAmount, 0);

            if (hasChoosenCirclePath)
                return;

            hasChoosenCirclePath = true;

            //  STRAFE LEFT? OR RIGHT?
            int leftOrRightIndex = Random.Range(0, 100);

            if (leftOrRightIndex >= 50)
            {
                //  LEFT
                strafeMoveAmount = -0.5f;
            }
            else
            {
                //  RIGHT
                strafeMoveAmount = 0.5f;
            }
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasAttack = false;
            hasEvaded = false;
            hasRolledForEvasionChance = false;
            hasRolledForComboChance = false;
            hasRolledForBlockChance = false;
            hasChoosenCirclePath = false;
            willBlockDuringThisCombatRotation = false;
            strafeMoveAmount = 0;
        }
    }
}
