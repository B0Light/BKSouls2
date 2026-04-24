using UnityEngine;

namespace BK
{
    public class AIMageCombatManager : AICharacterCombatManager
    {
        [Header("Spell Instantiation")]
        [Tooltip("주문 이펙트가 스폰될 위치. 지팡이 끝 또는 손 위치의 Transform 을 연결하세요.")]
        [SerializeField] private Transform spellInstantiationPoint;

        [Header("Mage Stats")]
        [Tooltip("AI 의 지능 레벨 (1~99). SpellItem.baseDamage 에 곱해지는 INT 배율을 결정합니다.")]
        [SerializeField, Range(1, 99)] private int intelligenceLevel = 40;
        [Tooltip("장비 무기의 SpellBuff 에 해당하는 마법 공격력 배율. 100 = 기본값.")]
        [SerializeField] private float spellBuff = 100f;

        /// <summary>현재 시전 중인 주문. AIMageSpellAction 에서 설정됩니다.</summary>
        public SpellItem currentSpell { get; set; }

        private GameObject activeWarmUpFX;

        // ─────────────────────────────────────────────────────────────────
        //  Animation Events
        // ─────────────────────────────────────────────────────────────────

        /// <summary>애니메이션 이벤트 — 시전 준비(Warm-up) FX를 spellInstantiationPoint 에 스폰합니다.</summary>
        public void InstantiateSpellWarmUpFX()
        {
            if (currentSpell == null || currentSpell.WarmUpFX == null)
                return;

            DestroyActiveWarmUpFX();

            Transform parent = spellInstantiationPoint != null ? spellInstantiationPoint : aiCharacter.transform;
            activeWarmUpFX = Instantiate(currentSpell.WarmUpFX, parent);
            activeWarmUpFX.transform.localPosition = Vector3.zero;
            activeWarmUpFX.transform.localRotation = Quaternion.identity;
        }

        /// <summary>애니메이션 이벤트 — 실제 주문 투사체를 발사합니다.</summary>
        public void SuccessfullyCastSpell()
        {
            if (currentSpell == null)
                return;

            DestroyActiveWarmUpFX();

            if (!aiCharacter.IsOwner)
                return;

            Transform spawnPoint = spellInstantiationPoint != null ? spellInstantiationPoint : aiCharacter.transform;
            float damage = CalculateSpellDamage(currentSpell);

            currentSpell.SuccessfullyCastSpell(aiCharacter, spawnPoint, damage);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Internal Helpers
        // ─────────────────────────────────────────────────────────────────

        private void DestroyActiveWarmUpFX()
        {
            if (activeWarmUpFX != null)
            {
                Destroy(activeWarmUpFX);
                activeWarmUpFX = null;
            }
        }

        /// <summary>
        /// AI용 주문 데미지 계산.
        /// INT 1 → ×1.01, INT 40 → ×1.33, INT 99 → ×1.50
        /// </summary>
        private float CalculateSpellDamage(SpellItem spell)
        {
            float normalized = Mathf.Clamp01(intelligenceLevel / 99f);
            float intMultiplier = 1f + (1f - Mathf.Pow(1f - normalized, 2f)) * 0.5f;
            return spell.baseDamage * (spellBuff / 100f) * intMultiplier;
        }
    }
}
