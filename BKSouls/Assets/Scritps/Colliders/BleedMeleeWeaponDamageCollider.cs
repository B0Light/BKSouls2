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
                characterCausingDamage.characterNetworkManager.NotifyServerOfBuildUpServerRpc(
                    damageTarget.NetworkObjectId,
                    (int)BuildUp.Bleed,
                    bleedBuildUpAmount);
            }
        }
    }
}
