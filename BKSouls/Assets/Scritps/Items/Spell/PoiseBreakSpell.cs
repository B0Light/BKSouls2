using UnityEngine;

namespace BK
{
    /// <summary>
    /// Poise Break 주문 (AOE 충격파).
    /// 시전자 발밑에 충격파를 생성해 소량의 매직 데미지와 함께 높은 Poise 데미지를 누적합니다.
    /// 동일한 대상에 여러 번 맞으면 Poise/Stance Break 를 유발합니다.
    /// 인스펙터의 spellCastReleaseFX 에 PoiseBreakSpellManager 컴포넌트가 달린 AOE 프리팹을 연결하세요.
    /// </summary>
    [CreateAssetMenu(menuName = "Items/Spells/Poise Break")]
    public class PoiseBreakSpell : SpellItem
    {
        [Header("Poise Damage")]
        [Tooltip("피격 시 대상의 Poise 에 누적되는 데미지. 여러 번 적중하면 스탠스 브레이크를 유발합니다.")]
        [SerializeField] private float poiseDamageAmount = 70f;

        // ─────────────────────────────────────────────────────────────────
        //  Player Casting
        // ─────────────────────────────────────────────────────────────────

        public override void AttemptToCastSpell(PlayerManager player)
        {
            base.AttemptToCastSpell(player);

            if (!CanICastThisSpell(player))
                return;

            if (player.playerNetworkManager.isUsingRightHand.Value)
                player.playerAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
            else
                player.playerAnimatorManager.PlayTargetActionAnimation(offHandSpellAnimation, true);
        }

        public override void InstantiateWarmUpSpellFX(PlayerManager player)
        {
            base.InstantiateWarmUpSpellFX(player);

            if (spellCastWarmUpFX == null)
                return;

            SpellInstantiationLocation spellInstantiationLocation = player.playerNetworkManager.isUsingRightHand.Value
                ? player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>()
                : player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();

            GameObject warmUpFX = Instantiate(spellCastWarmUpFX);
            warmUpFX.transform.parent = spellInstantiationLocation.transform;
            warmUpFX.transform.localPosition = Vector3.zero;
            warmUpFX.transform.localRotation = Quaternion.identity;

            player.playerEffectsManager.activeSpellWarmUpFX = warmUpFX;
        }

        public override void SuccessfullyCastSpell(PlayerManager player)
        {
            base.SuccessfullyCastSpell(player);

            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            //  충격파는 시전자 발밑에 AOE 로 생성 (라이트닝과 동일한 방식)
            SpawnAOE(player, player.transform.position, CalculateSpellDamage(player));
        }

        // ─────────────────────────────────────────────────────────────────
        //  AI Casting Overrides
        // ─────────────────────────────────────────────────────────────────

        public override void AttemptToCastSpell(AICharacterManager aiCharacter)
        {
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
        }

        /// <summary>
        /// spellSpawnPoint 는 AOE 특성상 사용되지 않습니다.
        /// 시전자의 현재 위치에 충격파를 생성합니다.
        /// </summary>
        public override void SuccessfullyCastSpell(AICharacterManager aiCharacter, Transform spellSpawnPoint, float damage)
        {
            if (spellCastReleaseFX == null)
                return;

            SpawnAOE(aiCharacter, aiCharacter.transform.position, damage);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Shared Spawn Logic
        // ─────────────────────────────────────────────────────────────────

        private void SpawnAOE(CharacterManager caster, Vector3 spawnPosition, float damage)
        {
            if (spellCastReleaseFX == null)
                return;

            GameObject aoe = Instantiate(spellCastReleaseFX, spawnPosition, Quaternion.identity);

            PoiseBreakSpellManager manager = aoe.GetComponent<PoiseBreakSpellManager>();
            if (manager != null)
                manager.InitializePoiseBreak(caster, damage, poiseDamageAmount);
        }
    }
}
