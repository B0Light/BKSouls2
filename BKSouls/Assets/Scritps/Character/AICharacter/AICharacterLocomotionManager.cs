using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BK
{
    public class AICharacterLocomotionManager : CharacterLocomotionManager
    {
        AICharacterManager aiCharacter;

        [Header("NavMesh Ground Correction")]
        [SerializeField] private bool correctFloatingBody = true;
        [SerializeField] private float allowedHeightAboveNavMesh = 0.35f;
        [SerializeField] private float groundCorrectionSpeed = 12f;
        [SerializeField] private float hardSnapHeight = 2.5f;
        [SerializeField] private float navMeshSampleDistance = 6f;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        public void RotateTowardsAgent(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterNetworkManager.isMoving.Value)
            {
                aiCharacter.transform.rotation = aiCharacter.navMeshAgent.transform.rotation;
            }
        }

        protected override void Update()
        {
            base.Update();
            CorrectFloatingBodyAboveNavMesh();

            if (aiCharacter.IsOwner)
            {
                aiCharacter.characterNetworkManager.verticalMovement.Value = aiCharacter.animator.GetFloat("Vertical");
                aiCharacter.characterNetworkManager.horizontalMovement.Value = aiCharacter.animator.GetFloat("Horizontal");
            }
            else
            {
                aiCharacter.animator.SetFloat("Vertical", aiCharacter.aiCharacterNetworkManager.verticalMovement.Value, 0.1f, Time.deltaTime);
                aiCharacter.animator.SetFloat("Horizontal", aiCharacter.aiCharacterNetworkManager.horizontalMovement.Value, 0.1f, Time.deltaTime);
            }
        }

        private void CorrectFloatingBodyAboveNavMesh()
        {
            if (!correctFloatingBody)
                return;

            if (!aiCharacter.IsOwner || aiCharacter.isDead.Value)
                return;

            if (aiCharacter.navMeshAgent == null || !aiCharacter.navMeshAgent.enabled)
                return;

            if (!NavMesh.SamplePosition(aiCharacter.transform.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                return;

            float heightAboveNavMesh = aiCharacter.transform.position.y - hit.position.y;
            if (heightAboveNavMesh <= allowedHeightAboveNavMesh)
                return;

            Vector3 correctedPosition = aiCharacter.transform.position;
            correctedPosition.y = hit.position.y;

            if (heightAboveNavMesh >= hardSnapHeight)
            {
                bool controllerWasEnabled = aiCharacter.characterController.enabled;
                aiCharacter.characterController.enabled = false;
                aiCharacter.transform.position = correctedPosition;
                aiCharacter.characterController.enabled = controllerWasEnabled;
            }
            else
            {
                float nextY = Mathf.MoveTowards(
                    aiCharacter.transform.position.y,
                    hit.position.y,
                    groundCorrectionSpeed * Time.deltaTime);

                aiCharacter.characterController.Move(new Vector3(0, nextY - aiCharacter.transform.position.y, 0));
            }

            yVelocity.y = groundedYVelocity;
            isGrounded = true;

            if (aiCharacter.navMeshAgent.isOnNavMesh)
                aiCharacter.navMeshAgent.Warp(aiCharacter.transform.position);
        }
    }
}
