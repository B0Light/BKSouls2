using UnityEngine;

namespace BK
{
    /// <summary>
    /// 원거리 공격 AI의 전투 매니저.
    /// - 투사체 발사 지점과 프리팹을 인스펙터에서 설정한다.
    /// - 애니메이션 이벤트에서 FireProjectile() 을 호출하면 타겟을 향해 투사체를 발사한다.
    /// </summary>
    public class AIRangedCharacterCombatManager : AICharacterCombatManager
    {
        [Header("Projectile Spawn")]
        [Tooltip("투사체가 생성될 위치 (없으면 캐릭터 루트 기준)")]
        [SerializeField] private Transform projectileSpawnPoint;

        [Header("Projectile Settings")]
        [Tooltip("발사할 투사체 프리팹 (SpellProjectileDamageCollider 또는 Rigidbody 포함)")]
        [SerializeField] private GameObject projectilePrefab;
        [Tooltip("투사체 직진 속도")]
        [SerializeField] private float projectileSpeed = 15f;
        [Tooltip("투사체 상향 속도 (포물선 보정)")]
        [SerializeField] private float projectileUpwardVelocity = 0.5f;

        // ─────────────────────────────────────────────────────────
        //  애니메이션 이벤트에서 호출
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// 애니메이션 이벤트로 호출됩니다.
        /// 현재 타겟을 향해 투사체를 발사합니다.
        /// </summary>
        public void FireProjectile()
        {
            if (!aiCharacter.IsOwner)
                return;

            if (currentTarget == null)
            {
                aiCharacter.animator.SetBool("isAttacking", false);
                return;
            }

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[AIRangedCharacterCombatManager] {aiCharacter.name}: 투사체 프리팹이 할당되지 않았습니다.");
                return;
            }

            Transform spawnPoint = projectileSpawnPoint != null
                ? projectileSpawnPoint
                : aiCharacter.transform;

            // 타겟의 잠금 포인트(Lock-on 위치)를 조준점으로 사용
            Vector3 aimPosition = currentTarget.characterCombatManager.lockOnTransform != null
                ? currentTarget.characterCombatManager.lockOnTransform.position
                : currentTarget.transform.position;

            // 투사체 생성 및 방향 설정
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
            Vector3 direction = (aimPosition - spawnPoint.position).normalized;
            projectile.transform.forward = direction;

            // 데미지 콜라이더 초기화
            SpellProjectileDamageCollider damageCollider = projectile.GetComponentInChildren<SpellProjectileDamageCollider>();
            if (damageCollider != null)
            {
                damageCollider.physicalDamage = baseDamage;
                damageCollider.poiseDamage    = basePoiseDamage;
                damageCollider.spellCaster    = aiCharacter;
            }

            // 물리 속도 적용
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed
                                  + Vector3.up * projectileUpwardVelocity;
            }
        }
    }
}
