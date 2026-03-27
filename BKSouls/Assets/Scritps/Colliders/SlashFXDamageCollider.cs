using UnityEngine;

namespace BK
{
    // Slash FX 프리팹에 붙이는 컴포넌트.
    // 검을 휘두를 때 SpawnSlashFX()로 월드에 생성되며, lifetime 동안 충돌 판정 후 자동 제거.
    // 프리팹 구성: Particle System + IsTrigger Collider + 이 스크립트
    public class SlashFXDamageCollider : DamageCollider
    {
        [Header("Slash FX Settings")]
        [Tooltip("공격자 CharacterManager (SpawnSlashFX에서 자동 설정됨)")]
        public CharacterManager characterCausingDamage;

        [Tooltip("기본 데미지에 곱할 배율 (0.5 = 무기 데미지의 50%)")]
        public float damageModifier = 0.5f;

        [Tooltip("이 FX가 존재하는 시간(초) 후 자동 제거")]
        public float lifetime = 0.35f;

        [Tooltip("AI 공격이면 true (피격자 IsOwner 기준 RPC), 플레이어면 false (공격자 IsOwner 기준)")]
        public bool isAIAttack = false;

        protected override void Awake()
        {
            base.Awake();
            damageCollider = GetComponent<Collider>();
            damageCollider.enabled = true;
            Destroy(gameObject, lifetime);
        }

        protected override void GetBlockingDotValues(CharacterManager damageTarget)
        {
            if (characterCausingDamage != null)
            {
                directionFromAttackToDamageTarget = characterCausingDamage.transform.position - damageTarget.transform.position;
                dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
            }
            else
            {
                base.GetBlockingDotValues(damageTarget);
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget == null)
                return;

            if (characterCausingDamage != null && damageTarget == characterCausingDamage)
                return;

            if (characterCausingDamage != null &&
                !WorldUtilityManager.Instance.CanIDamageThisTarget(characterCausingDamage.characterGroup, damageTarget.characterGroup))
                return;

            contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            CheckForBlock(damageTarget);

            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                DamageTarget(damageTarget);
        }

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage * damageModifier;
            damageEffect.magicDamage = magicDamage * damageModifier;
            damageEffect.fireDamage = fireDamage * damageModifier;
            damageEffect.holyDamage = holyDamage * damageModifier;
            damageEffect.poiseDamage = poiseDamage * damageModifier;
            damageEffect.contactPoint = contactPoint;

            if (characterCausingDamage != null)
                damageEffect.angleHitFrom = Vector3.SignedAngle(characterCausingDamage.transform.forward, damageTarget.transform.forward, Vector3.up);

            if (isAIAttack)
            {
                // AI: 피격자 클라이언트 기준으로 RPC 전송
                if (damageTarget.IsOwner)
                {
                    damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                        damageTarget.NetworkObjectId,
                        characterCausingDamage.NetworkObjectId,
                        damageEffect.physicalDamage,
                        damageEffect.magicDamage,
                        damageEffect.fireDamage,
                        damageEffect.holyDamage,
                        damageEffect.poiseDamage,
                        damageEffect.angleHitFrom,
                        damageEffect.contactPoint.x,
                        damageEffect.contactPoint.y,
                        damageEffect.contactPoint.z);
                }
            }
            else
            {
                // Player: 공격자 클라이언트 기준으로 RPC 전송
                if (characterCausingDamage != null && characterCausingDamage.IsOwner)
                {
                    damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                        damageTarget.NetworkObjectId,
                        characterCausingDamage.NetworkObjectId,
                        damageEffect.physicalDamage,
                        damageEffect.magicDamage,
                        damageEffect.fireDamage,
                        damageEffect.holyDamage,
                        damageEffect.poiseDamage,
                        damageEffect.angleHitFrom,
                        damageEffect.contactPoint.x,
                        damageEffect.contactPoint.y,
                        damageEffect.contactPoint.z);
                }
            }
        }
    }
}
