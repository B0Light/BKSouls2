using UnityEngine;
using UnityEngine.AI;

namespace BK
{
    /// <summary>
    /// 원거리 AI를 위한 전투 스탠스 상태.
    /// CombatStanceState의 공격 선택·콤보·방어·회피 로직을 그대로 사용하면서
    /// 타겟이 너무 가까이 오면 뒤로 물러나는 동작을 추가한다.
    /// </summary>
    [CreateAssetMenu(menuName = "A.I/States/Ranged Combat Stance")]
    public class AIRangedCombatStanceState : CombatStanceState
    {
        [Header("Range Keeping")]
        [Tooltip("이 거리보다 가까워지면 뒤로 물러납니다.")]
        [SerializeField] private float preferredMinDistance = 5f;

        [Tooltip("후퇴 목표 거리 (NavMesh 샘플링 반경으로도 사용)")]
        [SerializeField] private float backAwayDistance = 4f;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            // 기본 전투 스탠스 로직 실행 (공격 선택, 방어, 회피, 추적 전환 등)
            AIState nextState = base.Tick(aiCharacter);

            // 기본 로직이 다른 상태로 전환을 결정했으면 그대로 따른다
            if (nextState != this)
            {
                aiCharacter.animator.SetBool("isAttacking", false);
                aiCharacter.navMeshAgent.updateRotation = true;
                return nextState;
            }

            // 행동 중(공격·피격 등)이면 간섭하지 않는다
            if (aiCharacter.isPerformingAction)
            {
                aiCharacter.navMeshAgent.updateRotation = true;
                return this;
            }

            // 타겟이 너무 가까우면 후퇴
            if (aiCharacter.aiCharacterCombatManager.currentTarget != null &&
                aiCharacter.aiCharacterCombatManager.distanceFromTarget < preferredMinDistance)
            {
                BackAwayFromTarget(aiCharacter);
            }
            else
            {
                aiCharacter.navMeshAgent.updateRotation = true;
            }

            aiCharacter.animator.SetBool("isAttacking", true);
            return this;
        }

        /// <summary>
        /// 타겟 반대 방향으로 NavMesh 위에서 이동 목적지를 동기적으로 설정하고
        /// 플레이어를 바라보면서 후진 애니메이션을 재생한다.
        /// </summary>
        private void BackAwayFromTarget(AICharacterManager aiCharacter)
        {
            Vector3 directionAway = (aiCharacter.transform.position
                - aiCharacter.aiCharacterCombatManager.currentTarget.transform.position).normalized;

            Vector3 backAwayDestination = aiCharacter.transform.position + directionAway * backAwayDistance;

            // NavMesh 위의 유효한 위치로 샘플링
            if (NavMesh.SamplePosition(backAwayDestination, out NavMeshHit hit, backAwayDistance, NavMesh.AllAreas))
            {
                // CalculatePath + SetPath(동기)로 base.Tick이 설정한 플레이어 방향 경로를 즉시 덮어쓴다.
                // SetDestination(비동기)은 다음 프레임 base.Tick에 의해 다시 덮어씌워지므로 사용하지 않는다.
                if (aiCharacter.navMeshAgent.CalculatePath(hit.position, aiCharacter.runtimePath))
                    aiCharacter.navMeshAgent.SetPath(aiCharacter.runtimePath);
                else
                    aiCharacter.navMeshAgent.ResetPath();
            }
            else
            {
                aiCharacter.navMeshAgent.ResetPath();
            }

            // NavMesh 자동 회전을 끄고 플레이어를 직접 바라보게 한다.
            // (자동 회전을 켜두면 NavMesh가 후퇴 방향으로 회전시켜 뒷걸음 애니메이션이 플레이어 쪽을 향하게 됨)
            aiCharacter.navMeshAgent.updateRotation = false;
            Vector3 toTarget = aiCharacter.aiCharacterCombatManager.currentTarget.transform.position
                               - aiCharacter.transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(toTarget);
                aiCharacter.transform.rotation = Quaternion.Slerp(
                    aiCharacter.transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            // 후퇴 애니메이션 (Vertical = -1 : 뒤로 걷기)
            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, -1f);
        }
    }
}
