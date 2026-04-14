using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class LightningManager : SpellManager
    {
        // This script manages an AOE lightning strike that is instantiated at the caster's position.
        // On activation it briefly enables its damage collider to hit all nearby enemies, plays VFX, then destroys itself.

        [Header("Collider")]
        public LightningDamageCollider damageCollider;

        [Header("Settings")]
        [SerializeField] private float colliderActiveDuration = 0.4f;   //  HOW LONG THE DAMAGE COLLIDER STAYS ACTIVE

        public bool isFullyCharged = false;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            StartCoroutine(ActivateDamageCollider());
        }

        public void InitializeLightning(CharacterManager spellCaster, float calculatedDamage)
        {
            damageCollider.spellCaster = spellCaster;
            damageCollider.lightningDamage = calculatedDamage;

            if (isFullyCharged)
                damageCollider.lightningDamage *= 1.4f;
        }

        private IEnumerator ActivateDamageCollider()
        {
            damageCollider.EnableDamageCollider();

            yield return new WaitForSeconds(colliderActiveDuration);

            damageCollider.DisableDamageCollider();

            InstantiateLightningImpactFX();
        }

        public void InstantiateLightningImpactFX()
        {
            List<Vector3> positions = damageCollider.impactPositions.Count > 0
                ? damageCollider.impactPositions
                : new List<Vector3> { transform.position };

            foreach (Vector3 fxPosition in positions)
            {
                if (isFullyCharged && impactParticleFullCharge != null)
                    Instantiate(impactParticleFullCharge, fxPosition, Quaternion.identity);
                else if (impactParticle != null)
                    Instantiate(impactParticle, fxPosition, Quaternion.identity);

                WorldSoundFXManager.Instance.AlertNearbyCharactersToSound(fxPosition, 12);
            }

            Destroy(gameObject);
        }
    }
}
