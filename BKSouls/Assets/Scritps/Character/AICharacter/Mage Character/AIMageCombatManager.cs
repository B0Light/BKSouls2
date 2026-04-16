using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// 인스펙터에서 SpellItem 목록을 등록해 사용하는 AI 메이지 전투 매니저.
    ///
    /// 사용 흐름:
    ///   1. State Machine → SelectAndBeginCast()   (주문 선택 + 애니메이션 시작)
    ///   2. Animation Event → InstantiateWarmUpFX() (준비 이펙트 스폰)
    ///   3. Animation Event → CastSpell()           (투사체 발사)
    /// </summary>
    public class AIMageCombatManager : AICharacterCombatManager
    {
        // ─────────────────────────────────────────────────────────────────
        //  Inspector Fields
        // ─────────────────────────────────────────────────────────────────

        [Header("Spell Instantiation")]
        [Tooltip("주문 이펙트가 스폰될 위치. 지팡이 끝 또는 손 위치의 Transform 을 연결하세요.")]
        [SerializeField] private Transform spellInstantiationPoint;

        [Header("Mage Stats")]
        [Tooltip("AI 의 지능 레벨 (1~99). SpellItem.baseDamage 에 곱해지는 INT 배율을 결정합니다.")]
        [SerializeField, Range(1, 99)] private int intelligenceLevel = 40;
        [Tooltip("장비 무기의 SpellBuff 에 해당하는 마법 공격력 배율. 100 = 기본값.")]
        [SerializeField] private float spellBuff = 100f;

        [Header("Spell List")]
        [Tooltip("이 메이지가 사용할 수 있는 주문 목록. 인스펙터에서 SpellItem ScriptableObject 를 등록하세요.")]
        [SerializeField] private MageSpellEntry[] spells;

        // ─────────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────────

        /// <summary>현재 시전 중인 주문 (읽기 전용)</summary>
        public SpellItem currentSpell { get; private set; }

        private MageSpellEntry currentSpellEntry;
        private GameObject activeWarmUpFX;

        // ─────────────────────────────────────────────────────────────────
        //  State Machine Interface
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 현재 distanceFromTarget / viewableAngle 을 기준으로 유효한 주문을 가중치 랜덤 선택한 뒤,
        /// 해당 주문의 시전 애니메이션을 재생합니다.
        /// 선택된 주문은 currentSpell 에 저장됩니다.
        /// </summary>
        /// <returns>주문을 선택하고 시전을 시작했으면 true, 유효한 주문이 없으면 false</returns>
        public bool SelectAndBeginCast()
        {
            MageSpellEntry selected = SelectSpell();

            if (selected == null)
                return false;

            currentSpell.AttemptToCastSpell(aiCharacter);
            return true;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Animation Events
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 애니메이션 이벤트에서 호출하세요 — 시전 준비(Warm-up) FX를 spellInstantiationPoint 에 스폰합니다.
        /// </summary>
        public void InstantiateWarmUpFX()
        {
            if (currentSpell == null || currentSpell.WarmUpFX == null)
                return;

            DestroyActiveWarmUpFX();

            Transform parent = spellInstantiationPoint != null ? spellInstantiationPoint : aiCharacter.transform;
            activeWarmUpFX = Instantiate(currentSpell.WarmUpFX, parent);
            activeWarmUpFX.transform.localPosition = Vector3.zero;
            activeWarmUpFX.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// 애니메이션 이벤트에서 호출하세요 — 실제 주문 투사체를 발사합니다.
        /// IsOwner 가 아닌 클라이언트에서는 투사체 스폰을 건너뜁니다.
        /// </summary>
        public void CastSpell()
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

        /// <summary>
        /// distanceFromTarget 과 viewableAngle 조건을 만족하는 주문 중 가중치 랜덤으로 하나를 선택합니다.
        /// actionRecoveryTimer 도 자동으로 설정됩니다.
        /// </summary>
        private MageSpellEntry SelectSpell()
        {
            if (spells == null || spells.Length == 0)
                return null;

            List<MageSpellEntry> validSpells = new List<MageSpellEntry>();

            foreach (MageSpellEntry entry in spells)
            {
                if (entry.spell == null)
                    continue;

                if (distanceFromTarget < entry.minimumCastDistance || distanceFromTarget > entry.maximumCastDistance)
                    continue;

                if (viewableAngle < entry.minimumCastAngle || viewableAngle > entry.maximumCastAngle)
                    continue;

                validSpells.Add(entry);
            }

            if (validSpells.Count == 0)
                return null;

            int totalWeight = 0;
            foreach (MageSpellEntry entry in validSpells)
                totalWeight += entry.attackWeight;

            int roll = Random.Range(0, totalWeight);
            int accumulated = 0;

            foreach (MageSpellEntry entry in validSpells)
            {
                accumulated += entry.attackWeight;
                if (roll < accumulated)
                {
                    ApplySelection(entry);
                    return entry;
                }
            }

            // 부동소수점 오차 방어
            ApplySelection(validSpells[validSpells.Count - 1]);
            return currentSpellEntry;
        }

        private void ApplySelection(MageSpellEntry entry)
        {
            currentSpellEntry = entry;
            currentSpell = entry.spell;
            actionRecoveryTimer = entry.actionRecoveryTime;
        }

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
        /// SpellItem.CalculateSpellDamage 와 동일한 INT 스케일링 곡선을 사용합니다.
        /// INT 1 → ×1.01, INT 40 → ×1.33, INT 99 → ×1.50
        /// </summary>
        private float CalculateSpellDamage(SpellItem spell)
        {
            float normalized = Mathf.Clamp01(intelligenceLevel / 99f);
            float intMultiplier = 1f + (1f - Mathf.Pow(1f - normalized, 2f)) * 0.5f;
            return spell.baseDamage * (spellBuff / 100f) * intMultiplier;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Data
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// 인스펙터에서 메이지가 사용할 주문 하나를 설정하는 데이터 클래스.
    /// AIMageCombatManager 의 spells 배열 원소로 등록합니다.
    /// </summary>
    [System.Serializable]
    public class MageSpellEntry
    {
        [Tooltip("사용할 주문 ScriptableObject (FireBallSpell, LightningSpell 등)")]
        public SpellItem spell;

        [Header("Cast Distance")]
        [Tooltip("이 주문을 사용할 수 있는 최소 거리")]
        public float minimumCastDistance = 5f;
        [Tooltip("이 주문을 사용할 수 있는 최대 거리")]
        public float maximumCastDistance = 20f;

        [Header("Cast Angle")]
        [Tooltip("이 주문을 사용할 수 있는 최소 각도 (좌측 기준 음수)")]
        public float minimumCastAngle = -45f;
        [Tooltip("이 주문을 사용할 수 있는 최대 각도 (우측 기준 양수)")]
        public float maximumCastAngle = 45f;

        [Header("Action Values")]
        [Tooltip("이 주문이 선택될 가중치. 값이 클수록 자주 선택됩니다.")]
        public int attackWeight = 50;
        [Tooltip("이 주문 시전 후 다음 행동까지의 회복 대기 시간 (초)")]
        public float actionRecoveryTime = 2.5f;
    }
}
