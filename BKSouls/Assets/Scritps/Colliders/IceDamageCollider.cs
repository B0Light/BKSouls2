using UnityEngine;

namespace BK
{
    /// <summary>
    /// 아이스 투사체 전용 데미지 콜라이더.
    /// 피격 시 매직 데미지와 함께 Frost 빌드업을 대상에게 누적합니다.
    /// </summary>
    public class IceDamageCollider : SpellProjectileDamageCollider
    {
        private IceSpellManager iceSpellManager;

        [Header("Frost Build Up")]
        [Tooltip("직접 피격 시 대상에게 누적되는 Frost 빌드업 수치")]
        public int frostBuildUpAmount = 25;

        [Header("Frost Splash")]
        [Tooltip("비플레이어 대상 충돌 시 빙결이 퍼지는 반경")]
        public float splashRadius = 4f;
        [Tooltip("범위 내 대상에게 누적되는 Frost 빌드업 수치")]
        public int splashFrostBuildUpAmount = 15;

        protected override void Awake()
        {
            base.Awake();
            iceSpellManager = GetComponentInParent<IceSpellManager>();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget == null)
                return;

            contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (damageTarget == spellCaster)
                return;

            if (!WorldUtilityManager.Instance.CanIDamageThisTarget(spellCaster.characterGroup, damageTarget.characterGroup))
                return;

            CheckForBlock(damageTarget);

            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                DamageTarget(damageTarget);

            if (!(damageTarget is PlayerManager))
                ApplyFrostSplash(contactPoint);

            iceSpellManager.InstantiateSpellDestructionFX();
        }

        protected override void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = spellCaster.transform.position - damageTarget.transform.position;
            dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
        }

        private static readonly Collider[] _splashBuffer = new Collider[16];

        private void ApplyFrostSplash(Vector3 center)
        {
            if (!spellCaster.IsOwner)
                return;

            int count = Physics.OverlapSphereNonAlloc(center, splashRadius, _splashBuffer, WorldUtilityManager.Instance.GetCharacterLayers());
            for (int i = 0; i < count; i++)
            {
                Collider hit = _splashBuffer[i];
                CharacterManager target = hit.GetComponentInParent<CharacterManager>();
                if (target == null || target == spellCaster)
                    continue;
                if (!WorldUtilityManager.Instance.CanIDamageThisTarget(spellCaster.characterGroup, target.characterGroup))
                    continue;
                if (charactersDamaged.Contains(target))
                    continue;

                if (splashFrostBuildUpAmount > 0)
                {
                    TakeBuildUpEffect frostEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeFrostBuildUpEffect);
                    frostEffect.buildUpAmount = splashFrostBuildUpAmount;
                    target.characterEffectsManager.ProcessInstantEffect(frostEffect);
                }
            }
        }

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicDamage = magicDamage;
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            damageEffect.angleHitFrom = Vector3.SignedAngle(spellCaster.transform.forward, damageTarget.transform.forward, Vector3.up);

            if (spellCaster.IsOwner)
            {
                //  데미지 전송 (기존 RPC 경로)
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    spellCaster.NetworkObjectId,
                    damageEffect.physicalDamage,
                    damageEffect.magicDamage,
                    damageEffect.fireDamage,
                    damageEffect.holyDamage,
                    damageEffect.poiseDamage,
                    damageEffect.angleHitFrom,
                    damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y,
                    damageEffect.contactPoint.z);

                //  Frost 빌드업 누적 (서버 권한으로 직접 적용)
                //  NOTE: AI는 서버 소유이므로 서버에서 직접 적용 가능.
                //        플레이어 시전자 → 플레이어 피격 시에는 별도 RPC 확장 필요.
                if (frostBuildUpAmount > 0)
                {
                    TakeBuildUpEffect frostEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeFrostBuildUpEffect);
                    frostEffect.buildUpAmount = frostBuildUpAmount;
                    damageTarget.characterEffectsManager.ProcessInstantEffect(frostEffect);
                }
            }
        }
    }
}
