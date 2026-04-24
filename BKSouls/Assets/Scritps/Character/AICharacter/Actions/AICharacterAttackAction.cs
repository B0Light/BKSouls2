using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "A.I/Actions/Attack")]
    public class AICharacterAttackAction : ScriptableObject
    {
        [Header("Attack")]
        [SerializeField] protected string attackAnimation;
        [SerializeField] bool isParryable = true;

        [Header("Combo Action")]
        public AICharacterAttackAction comboAction; // The combo action of this attack action

        [Header("Action Values")]
        [SerializeField] AttackType attackType;
        public int attackWeight = 50;
        // ATTACK CAN BE REPEATED
        public float actionRecoveryTime = 1.5f; //  The time before the character can make another attack after performing this one
        public float minimumAttackAngle = -35;
        public float maximumAttackAngle = 35;
        public float minimumAttackDistance = 0;
        public float maximumAttackDistance = 2;

        public virtual void AttemptToPerformAction(AICharacterManager aiCharacter)
        {
            //  DOES YOUR A.I ACT LIKE A PLAYER CHARACTER (LIKE AN INVADER A.I?) IF SO USE THIS
            //aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(attackType, attackAnimation, true);

            //  DOES YOUR A.I USE SIMPLE ATTACKS THAT ARE PURELY ANIMATION BASED (NOT EQUIPMENT / ITEM BASED) IF SO USE THIS
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(attackAnimation, true);
            aiCharacter.aiCharacterNetworkManager.isParryable.Value = isParryable;
        }
    }
}
