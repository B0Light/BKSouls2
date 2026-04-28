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
        [Tooltip("모션별 Slash FX 프리팹 배열. Animation Event의 int 파라미터로 인덱스 지정\n예) 0=Attack01, 1=Attack02, 2=HeavyAttack")]
        [SerializeField] GameObject[] slashFXPrefabs;
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

        // 애니메이션 이벤트에서 호출 — prefabIndex로 모션별 다른 VFX 선택
        // Animation Event 설정: Function = SpawnSlashFX, Int = 0 (Attack01), 1 (Attack02), ...
        public void SpawnSlashFX(int prefabIndex = 0)
        {
            if (slashFXPrefabs == null || prefabIndex < 0 || prefabIndex >= slashFXPrefabs.Length)
                return;

            GameObject prefab = slashFXPrefabs[prefabIndex];
            if (prefab == null)
                return;

            Transform spawnPoint = slashFXSpawnPoint != null ? slashFXSpawnPoint : aiCharacter.transform;
            GameObject fx = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            SlashFXDamageCollider slashCollider = fx.GetComponentInChildren<SlashFXDamageCollider>();
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
