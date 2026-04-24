using UnityEngine;

namespace BK
{
    public class BleedMeleeWeaponDamageCollider : MeleeWeaponDamageCollider
    {
        [Header("Bleed Build Up")]
        public int bleedBuildUpAmount = 50;

        protected override void DamageTarget(CharacterManager damageTarget)
        {
            base.DamageTarget(damageTarget);

            if (bleedBuildUpAmount > 0 && characterCausingDamage.IsOwner)
            {
                TakeBuildUpEffect bleedEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeBleedBuildUpEffect);
                bleedEffect.buildUpAmount = bleedBuildUpAmount;
                damageTarget.characterEffectsManager.ProcessInstantEffect(bleedEffect);
            }
        }
    }
}
