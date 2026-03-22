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
                return nextState;

            // 행동 중(공격·피격 등)이면 간섭하지 않는다
            if (aiCharacter.isPerformingAction)
                return this;

            // 타겟이 너무 가까우면 후퇴
            if (aiCharacter.aiCharacterCombatManager.currentTarget != null &&
                aiCharacter.aiCharacterCombatManager.distanceFromTarget < preferredMinDistance)
            {
                BackAwayFromTarget(aiCharacter);
            }

            return this;
        }

        /// <summary>
        /// 타겟 반대 방향으로 NavMesh 위에서 이동 목적지를 설정하고
        /// 애니메이터 파라미터를 후진(-1)으로 설정한다.
        /// </summary>
        private void BackAwayFromTarget(AICharacterManager aiCharacter)
        {
            Vector3 directionAway = (aiCharacter.transform.position
                - aiCharacter.aiCharacterCombatManager.currentTarget.transform.position).normalized;

            Vector3 backAwayDestination = aiCharacter.transform.position + directionAway * backAwayDistance;

            // NavMesh 위의 유효한 위치로 샘플링
            if (NavMesh.SamplePosition(backAwayDestination, out NavMeshHit hit, backAwayDistance, NavMesh.AllAreas))
            {
                aiCharacter.navMeshAgent.SetDestination(hit.position);
            }

            // 후퇴 애니메이션 (Vertical = -1 : 뒤로 걷기)
            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, -1f);
        }
    }
}
