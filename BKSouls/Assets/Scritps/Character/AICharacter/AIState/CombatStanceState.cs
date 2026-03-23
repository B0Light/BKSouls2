using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BK
{
    [CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
    public class CombatStanceState : AIState
    {
        [Header("공격(Attacks)")]
        public List<AICharacterAttackAction> aiCharacterAttacks;                   // 이 AI가 사용할 수 있는 전체 공격 목록
        [SerializeField] protected List<AICharacterAttackAction> potentialAttacks; // 현재 상황(거리/각도 등)에서 가능한 공격 후보
        [SerializeField] private AICharacterAttackAction chosenAttack;             // 최종 선택된 공격
        [SerializeField] private AICharacterAttackAction previousAttack;           // 이전 공격(연속 중복 방지 등에 사용 가능)
        protected bool hasAttack = false;

        [Header("콤보(Combo)")]
        [SerializeField] protected bool canPerformCombo = false;                 // 콤보를 시도할 수 있는지
        [SerializeField] protected int percentageOfTimeWillPerformCombo = 25;    // 콤보 시도 확률(%)
        [SerializeField] public bool onlyPerformComboIfInitialAttackHits = false; // 첫 타가 맞았을 때만 콤보(외부 로직과 연계 가능)
        protected bool hasRolledForComboChance = false;                          // 이 상태에서 이미 콤보 확률을 굴렸는지

        [Header("교전 거리(Engagement Distance)")]
        [SerializeField] public float maximumEngagementDistance = 5f;            // 이 거리보다 멀면 추적 상태로 전환

        [Header("원형 이동(Circling)")]
        [SerializeField] private bool willCircleTarget = false;                  // 타겟을 중심으로 좌/우 원형 이동을 할지
        private bool hasChosenCirclePath = false;                                // 이미 좌/우 방향을 정했는지
        private float strafeMoveAmount = 0f;                                     // -0.5: 왼쪽, +0.5: 오른쪽

        [Header("방어(Blocking)")]
        [SerializeField] private bool canBlock = false;                          // 방어 가능 여부
        [SerializeField] private int percentageOfTimeWillBlock = 75;             // 방어 확률(%)
        private bool hasRolledForBlockChance = false;                            // 이 상태에서 이미 방어 확률을 굴렸는지
        private bool willBlockDuringThisCombatRotation = false;                  // 이번 전투 루프에서 방어를 할지

        [Header("회피(Evasion)")]
        [SerializeField] private bool canEvade = false;                          // 회피 가능 여부
        [SerializeField] private int percentageOfTimeWillEvade = 75;             // 회피 확률(%)
        private bool hasEvaded = false;                                          // 이미 회피를 수행했는지(연속 회피 방지)
        private bool hasRolledForEvasionChance = false;                          // 이 상태에서 이미 회피 확률을 굴렸는지
        private bool willEvadeDuringThisCombatRotation = false;                  // 이번 전투 루프에서 회피를 할지

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            // 행동 중이면(공격/피격/회피 등) 현재 상태 유지
            if (aiCharacter.isPerformingAction)
                return this;

            // 타겟이 없으면 바로 Idle로 복귀 (아래에서 null 참조 방지)
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);
            
            var targetPos = aiCharacter.aiCharacterCombatManager.currentTarget.transform.position;
            
            // NavMeshAgent가 꺼져있으면 켠다
            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;

            // 타겟이 사망했으면 타겟 해제
            if (aiCharacter.aiCharacterCombatManager.currentTarget.isDead.Value)
                aiCharacter.aiCharacterCombatManager.SetTarget(null);

            // enablePivot 옵션: 시야각 밖에 있으면 제자리에서 타겟을 향해 피벗 회전
            if (aiCharacter.aiCharacterCombatManager.enablePivot)
            {
                if (!aiCharacter.aiCharacterNetworkManager.isMoving.Value)
                {
                    float angle = aiCharacter.aiCharacterCombatManager.viewableAngle;
                    if (angle < -30f || angle > 30f)
                        aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);
                }
            }

            // NavMeshAgent 방향을 따라 회전(이동 중 자연스러운 추적)
            aiCharacter.aiCharacterCombatManager.RotateTowardsAgent(aiCharacter);

            // 원형 이동(스트레이프) 설정
            if (willCircleTarget)
                SetCirclePath(aiCharacter);

            // 이번 전투 루프에서 방어/회피/콤보를 할지 확률을 1번만 결정
            RollCombatChances(aiCharacter);

            // 방어 플래그 적용
            if (willBlockDuringThisCombatRotation)
                aiCharacter.aiCharacterNetworkManager.isBlocking.Value = true;

            // 회피: 타겟이 공격 중이고 아직 회피하지 않았다면 1회 수행
            if (willEvadeDuringThisCombatRotation && aiCharacter.aiCharacterCombatManager.currentTarget != null &&
                aiCharacter.aiCharacterCombatManager.currentTarget.characterNetworkManager.isAttacking.Value &&
                !hasEvaded)
            {
                hasEvaded = true;
                aiCharacter.aiCharacterCombatManager.PerformEvasion();
            }

            // 공격이 아직 없으면 새 공격 선택
            if (!hasAttack)
            {
                ChooseNewAttack(aiCharacter);
            }
            else
            {
                // 공격 상태로 전환
                aiCharacter.attack.currentAttack = chosenAttack;
                return SwitchState(aiCharacter, aiCharacter.attack);
            }

            // 교전 거리 밖이면 추적으로 전환
            if (aiCharacter.aiCharacterCombatManager.distanceFromTarget > maximumEngagementDistance)
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);

            // 타겟 위치로 경로 계산 및 적용
            
            
            if (aiCharacter.navMeshAgent.CalculatePath(targetPos, aiCharacter.runtimePath))
            {
                aiCharacter.navMeshAgent.SetPath(aiCharacter.runtimePath);
            }

            return this;
        }

        /// <summary>
        /// 방어/회피/콤보를 이번 전투 로테이션에서 수행할지 1회만 굴린다.
        /// </summary>
        private void RollCombatChances(AICharacterManager aiCharacter)
        {
            // 방어 확률
            if (canBlock && !hasRolledForBlockChance)
            {
                hasRolledForBlockChance = true;
                willBlockDuringThisCombatRotation = RollForOutcomeChance(percentageOfTimeWillBlock);
            }

            // 회피 확률
            if (canEvade && !hasRolledForEvasionChance)
            {
                hasRolledForEvasionChance = true;
                willEvadeDuringThisCombatRotation = RollForOutcomeChance(percentageOfTimeWillEvade);
            }

            // 콤보 확률
            if (canPerformCombo && !hasRolledForComboChance)
            {
                hasRolledForComboChance = true;
                aiCharacter.attack.willPerformCombo = RollForOutcomeChance(percentageOfTimeWillPerformCombo);
            }
        }

        /// <summary>
        /// 현재 거리/각도 조건에서 가능한 공격 후보를 모아 가중치 랜덤으로 1개 선택한다.
        /// </summary>
        protected virtual void ChooseNewAttack(AICharacterManager aiCharacter)
        {
            if (potentialAttacks == null)
                potentialAttacks = new List<AICharacterAttackAction>(16);
            else
                potentialAttacks.Clear();

            float distance = aiCharacter.aiCharacterCombatManager.distanceFromTarget;
            float angle = aiCharacter.aiCharacterCombatManager.viewableAngle;

            foreach (var attack in aiCharacterAttacks)
            {
                // 너무 가까우면 제외
                if (attack.minimumAttackDistance > distance)
                {
                    Debug.Log("사거리 미만");
                    continue;
                }

                // 너무 멀면 제외
                if (attack.maximumAttackDistance < distance)
                {
                    Debug.Log("사거리 초과");
                    continue;
                }

                // 시야각이 최소 각도보다 작으면 제외
                if (attack.minimumAttackAngle > angle)
                {
                    Debug.Log("시야각 미만");
                    continue;
                }

                // 시야각이 최대 각도보다 크면 제외
                if (attack.maximumAttackAngle < angle)
                {
                    Debug.Log("시야각 초과");
                    continue;
                }

                potentialAttacks.Add(attack);
            }

            if (potentialAttacks.Count == 0)
            {
                Debug.Log("No Potential Attacks");
                return;
            }

            // 후보들의 가중치 합
            int totalWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
                totalWeight += potentialAttacks[i].attackWeight;

            // 1 ~ totalWeight 범위
            int randomWeightValue = Random.Range(1, totalWeight + 1);

            // 누적 가중치로 선택
            int processedWeight = 0;
            for (int i = 0; i < potentialAttacks.Count; i++)
            {
                var attack = potentialAttacks[i];
                processedWeight += attack.attackWeight;

                if (randomWeightValue <= processedWeight)
                {
                    chosenAttack = attack;

                    // 필요하다면 "연속 같은 공격 방지" 같은 로직을 여기에 추가 가능
                    previousAttack = chosenAttack;

                    hasAttack = true;
                    return;
                }
            }
        }

        /// <summary>
        /// outcomeChance(%) 확률로 true를 반환한다.
        /// </summary>
        protected virtual bool RollForOutcomeChance(int outcomeChance)
        {
            int randomPercentage = Random.Range(0, 100);
            return randomPercentage < outcomeChance;
        }

        /// <summary>
        /// 타겟을 중심으로 좌/우 스트레이프 이동 파라미터를 설정한다.
        /// 장애물과 충돌 중이면 스트레이프를 중단하고 전진 추적으로 전환되게 만든다.
        /// </summary>
        protected virtual void SetCirclePath(AICharacterManager aiCharacter)
        {
            // 스트레이프 중 장애물과 충돌하면 스트레이프 중단
            bool isColliding = Physics.CheckSphere(
                aiCharacter.aiCharacterCombatManager.lockOnTransform.position,
                aiCharacter.characterController.radius + 0.25f,
                WorldUtilityManager.Instance.GetEnviroLayers()
            );

            if (isColliding)
            {
                // 스트레이프를 중단하고(좌/우 0), 전진(절대값)으로 전환 느낌을 준다
                aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, Mathf.Abs(strafeMoveAmount));
                return;
            }

            // 스트레이프 유지
            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(strafeMoveAmount, 0);

            // 좌/우 방향은 1번만 결정
            if (hasChosenCirclePath)
                return;

            hasChosenCirclePath = true;

            // 50% 확률로 좌/우
            int leftOrRightIndex = Random.Range(0, 100);
            strafeMoveAmount = (leftOrRightIndex >= 50) ? -0.5f : 0.5f;
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasAttack = false;
            hasEvaded = false;

            hasRolledForEvasionChance = false;
            hasRolledForComboChance = false;
            hasRolledForBlockChance = false;

            hasChosenCirclePath = false;

            willBlockDuringThisCombatRotation = false;
            willEvadeDuringThisCombatRotation = false;

            strafeMoveAmount = 0f;
        }
    }
}