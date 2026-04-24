using UnityEngine;

namespace BK
{
    public class PoisonSpellManager : SpellManager
    {
        [Header("Collider")]
        public PoisonDamageCollider damageCollider;

        private bool hasCollided = false;

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitializePoisonSpell(CharacterManager spellCaster, float calculatedDamage)
        {
            damageCollider.spellCaster = spellCaster;
            damageCollider.magicDamage = calculatedDamage;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 6)
                return;

            if (!hasCollided)
            {
                hasCollided = true;
                InstantiateSpellDestructionFX();
            }
        }

        public void InstantiateSpellDestructionFX()
        {
            if (impactParticle != null)
                Instantiate(impactParticle, transform.position, Quaternion.identity);

            WorldSoundFXManager.Instance.AlertNearbyCharactersToSound(transform.position, 6);

            Destroy(gameObject);
        }
    }
}
