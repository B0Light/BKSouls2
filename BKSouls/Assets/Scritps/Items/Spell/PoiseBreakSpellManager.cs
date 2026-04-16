using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// Poise Break AOE 프리팹의 런타임 관리자.
    /// 스폰 즉시 데미지 콜라이더를 활성화하고, 일정 시간 후 비활성화한 뒤 이펙트를 생성합니다.
    /// LightningManager 와 동일한 흐름을 따릅니다.
    /// </summary>
    public class PoiseBreakSpellManager : SpellManager
    {
        [Header("Collider")]
        public PoiseBreakDamageCollider damageCollider;

        [Header("Settings")]
        [Tooltip("데미지 콜라이더가 활성 상태로 유지되는 시간 (초)")]
        [SerializeField] private float colliderActiveDuration = 0.35f;

        protected override void Start()
        {
            base.Start();
            StartCoroutine(ActivateDamageCollider());
        }

        /// <summary>시전자, 계산된 데미지, Poise 데미지 수치를 콜라이더에 주입합니다.</summary>
        public void InitializePoiseBreak(CharacterManager spellCaster, float calculatedDamage, float poiseDamageAmount)
        {
            damageCollider.spellCaster = spellCaster;
            damageCollider.magicDamage = calculatedDamage;
            damageCollider.poiseDamage = poiseDamageAmount;
        }

        private IEnumerator ActivateDamageCollider()
        {
            damageCollider.EnableDamageCollider();

            yield return new WaitForSeconds(colliderActiveDuration);

            damageCollider.DisableDamageCollider();

            InstantiateSpellDestructionFX();
        }

        public void InstantiateSpellDestructionFX()
        {
            List<Vector3> positions = damageCollider.impactPositions.Count > 0
                ? damageCollider.impactPositions
                : new List<Vector3> { transform.position };

            foreach (Vector3 fxPosition in positions)
            {
                if (impactParticle != null)
                    Instantiate(impactParticle, fxPosition, Quaternion.identity);

                WorldSoundFXManager.Instance.AlertNearbyCharactersToSound(fxPosition, 10);
            }

            Destroy(gameObject);
        }
    }
}
