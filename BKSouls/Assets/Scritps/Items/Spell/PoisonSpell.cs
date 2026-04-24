using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Items/Spells/Poison")]
    public class PoisonSpell : SpellItem
    {
        [Header("Projectile Velocity")]
        [SerializeField] private float upwardVelocity = 1f;
        [SerializeField] private float forwardVelocity = 20f;

        [Header("Poison Build Up")]
        [Tooltip("투사체 피격 시 대상에게 누적되는 Poison 빌드업 수치")]
        [SerializeField] private int poisonBuildUpAmount = 30;

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

            Transform spawnPoint = GetSpawnPointForPlayer(player);
            CharacterManager target = player.playerNetworkManager.isLockedOn.Value
                ? player.playerCombatManager.currentTarget
                : null;

            SpawnProjectile(player, spawnPoint, CalculateSpellDamage(player), target);
        }

        public override void AttemptToCastSpell(AICharacterManager aiCharacter)
        {
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
        }

        public override void SuccessfullyCastSpell(AICharacterManager aiCharacter, Transform spellSpawnPoint, float damage)
        {
            if (spellCastReleaseFX == null)
                return;

            CharacterManager target = aiCharacter.characterCombatManager.currentTarget;
            SpawnProjectile(aiCharacter, spellSpawnPoint, damage, target);
        }

        private void SpawnProjectile(CharacterManager caster, Transform spawnPoint, float damage, CharacterManager target)
        {
            if (spellCastReleaseFX == null)
                return;

            GameObject projectile = Instantiate(spellCastReleaseFX, spawnPoint.position, spawnPoint.rotation);
            projectile.transform.parent = null;

            PoisonSpellManager poisonManager = projectile.GetComponent<PoisonSpellManager>();
            if (poisonManager != null)
            {
                poisonManager.InitializePoisonSpell(caster, damage);
                poisonManager.damageCollider.poisonBuildUpAmount = poisonBuildUpAmount;
            }

            if (target != null)
                projectile.transform.LookAt(target.characterCombatManager.lockOnTransform.position);
            else
                projectile.transform.forward = caster.transform.forward;

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = projectile.transform.forward * forwardVelocity
                                  + projectile.transform.up * upwardVelocity;
            }
        }

        private Transform GetSpawnPointForPlayer(PlayerManager player)
        {
            bool useRight = player.playerNetworkManager.isUsingRightHand.Value
                         || player.playerNetworkManager.isTwoHandingRightWeapon.Value;

            SpellInstantiationLocation loc = useRight
                ? player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>()
                : player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();

            return loc != null ? loc.transform : player.transform;
        }
    }
}
