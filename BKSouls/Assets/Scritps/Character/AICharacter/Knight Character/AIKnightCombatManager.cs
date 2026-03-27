using UnityEngine;

namespace BK
{
    public class AIKnightCombatManager : AICharacterCombatManager
    {
        [Header("Damage Colliders")]
        [SerializeField] ManualDamageCollider swordDamageCollider;

        [Header("Damage Modifiers")]
        [SerializeField] float attack01DamageModifier = 1.0f;
        [SerializeField] float attack02DamageModifier = 1.4f;

        [Header("Slash FX")]
        [Tooltip("SlashFXDamageCollider + Particle System이 붙은 프리팹")]
        [SerializeField] GameObject slashFXPrefab;
        [Tooltip("검기가 생성될 위치 (보통 검 끝 혹은 캐릭터 앞). 비워두면 캐릭터 위치 사용")]
        [SerializeField] Transform slashFXSpawnPoint;
        [Tooltip("검기 데미지 배율 (baseDamage 기준)")]
        [SerializeField] float slashFXDamageModifier = 0.5f;

        public void SetAttack01Damage()
        {
            swordDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            swordDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
        }

        public void SetAttack02Damage()
        {
            swordDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
            swordDamageCollider.poiseDamage = basePoiseDamage * attack02DamageModifier;
        }

        public void OpenSwordDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();
            swordDamageCollider.EnableDamageCollider();
        }

        public void CloseSwordDamageCollider()
        {
            swordDamageCollider.DisableDamageCollider();
        }

        public override void CloseAllDamageColliders()
        {
            base.CloseAllDamageColliders();

            swordDamageCollider.DisableDamageCollider();
        }

        // 애니메이션 이벤트에서 호출 — 검기 FX를 월드에 스폰하고 데미지 판정 설정
        public void SpawnSlashFX()
        {
            if (slashFXPrefab == null)
                return;

            Transform spawnPoint = slashFXSpawnPoint != null ? slashFXSpawnPoint : aiCharacter.transform;
            GameObject fx = Instantiate(slashFXPrefab, spawnPoint.position, spawnPoint.rotation);

            SlashFXDamageCollider slashCollider = fx.GetComponent<SlashFXDamageCollider>();
            if (slashCollider != null)
            {
                slashCollider.characterCausingDamage = aiCharacter;
                slashCollider.physicalDamage = baseDamage;
                slashCollider.poiseDamage = basePoiseDamage;
                slashCollider.damageModifier = slashFXDamageModifier;
                slashCollider.isAIAttack = true;
            }
        }
    }
}
