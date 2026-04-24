using UnityEngine;

namespace BK
{
    public class PoisonArrowDamageCollider : RangedProjectileDamageCollider
    {
        [Header("Poison Build Up")]
        public int poisonBuildUpAmount = 40;

        protected override void OnCollisionEnter(Collision collision)
        {
            CharacterManager potentialTarget = collision.transform.gameObject.GetComponent<CharacterManager>();

            if (characterShootingProjectile != null && potentialTarget != null)
            {
                if (WorldUtilityManager.Instance.CanIDamageThisTarget(characterShootingProjectile.characterGroup, potentialTarget.characterGroup))
                {
                    if (poisonBuildUpAmount > 0 && characterShootingProjectile.IsOwner)
                    {
                        TakeBuildUpEffect poisonEffect = Instantiate(WorldCharacterEffectsManager.Instance.takePoisonBuildUpEffect);
                        poisonEffect.buildUpAmount = poisonBuildUpAmount;
                        potentialTarget.characterEffectsManager.ProcessInstantEffect(poisonEffect);
                    }
                }
            }

            base.OnCollisionEnter(collision);
        }
    }
}
