using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class LightningDamageCollider : SpellProjectileDamageCollider
    {
        protected override void Awake()
        {
            base.Awake();
        }

        [SerializeField] private float damageDelay = 1f;

        public List<Vector3> impactPositions = new List<Vector3>();

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
                StartCoroutine(DelayedDamage(damageTarget, damageDelay));
            }
        }

        private IEnumerator DelayedDamage(CharacterManager damageTarget, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (damageTarget == null)
                yield break;

            DamageTarget(damageTarget);
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
            damageEffect.lightningDamage = lightningDamage;
            damageEffect.holyDamage = holyDamage;
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            damageEffect.angleHitFrom = Vector3.SignedAngle(spellCaster.transform.up, damageTarget.transform.up, Vector3.up);

            if (spellCaster.IsOwner)
            {
                //  NOTE: lightningDamage is passed via the holyDamage slot because the server RPC does not have a dedicated lightningDamage parameter.
                //  To properly support lightningDamage over the network, extend NotifyTheServerOfCharacterDamageServerRpc with a lightningDamage parameter.
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    spellCaster.NetworkObjectId,
                    damageEffect.physicalDamage,
                    damageEffect.magicDamage,
                    damageEffect.fireDamage,
                    damageEffect.lightningDamage,   //  MAPPED TO holyDamage SLOT — SEE NOTE ABOVE
                    damageEffect.poiseDamage,
                    damageEffect.angleHitFrom,
                    damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y,
                    damageEffect.contactPoint.z);
            }
        }
    }
}
