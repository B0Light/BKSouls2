using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BK
{
    public class AIState : ScriptableObject
    {
        public virtual AIState Tick(AICharacterManager aiCharacter)
        {
            return this;
        }

        public virtual AIState SwitchState(AICharacterManager aiCharacter, AIState newState)
        {
            ResetStateFlags(aiCharacter);
            return newState;
        }

        protected virtual void ResetStateFlags(AICharacterManager aiCharacter)
        {
            //  RESET ANY STATE FLAGS HERE SO WHEN YOU RETURN TO THE STATE, THEY ARE BLANK ONCE AGAIN
        }

        public bool IsDestinationReachable(AICharacterManager aiCharacter, Vector3 destination)
        {
            aiCharacter.navMeshAgent.enabled = true;

            NavMeshPath navMeshPath = new NavMeshPath();

            if (aiCharacter.navMeshAgent.CalculatePath(destination, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
