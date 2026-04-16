using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// Poise Break 주문 전용 AOE 데미지 콜라이더.
    /// 피격 시 소량의 매직 데미지와 함께 높은 poiseDamage 를 전송하여
    /// 대상의 Poise(자세)를 누적 파괴합니다.
    /// LightningDamageCollider 와 동일한 구조를 따릅니다.
    /// </summary>
    public class PoiseBreakDamageCollider : SpellProjectileDamageCollider
    {
        /// <summary>피격 위치 목록 — PoiseBreakSpellManager 의 이펙트 스폰에 사용됩니다.</summary>
        public List<Vector3> impactPositions = new List<Vector3>();

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget == null)
                return;

            if (damageTarget == spellCaster)
                return;

            if (!WorldUtilityManager.Instance.CanIDamageThisTarget(spellCaster.characterGroup, damageTarget.characterGroup))
                return;

            CheckForBlock(damageTarget);

            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
            {
                contactPoint = damageTarget.transform.position;
                impactPositions.Add(contactPoint);
                DamageTarget(damageTarget);
            }
        }

        protected override void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = spellCaster.transform.position - damageTarget.transform.position;
            dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
        }

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicDamage = magicDamage;
            //  poiseDamage 를 높게 설정하여 자세 파괴 누적
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            damageEffect.angleHitFrom = Vector3.SignedAngle(spellCaster.transform.up, damageTarget.transform.up, Vector3.up);

            if (spellCaster.IsOwner)
            {
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
            }
        }
    }
}
