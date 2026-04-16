using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Items/Spells/Lightning")]
    public class LightningSpell : SpellItem
    {
        public override void AttemptToCastSpell(PlayerManager player)
        {
            base.AttemptToCastSpell(player);

            if (!CanICastThisSpell(player))
                return;

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
            }
            else
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(offHandSpellAnimation, true);
            }
        }

        public override void InstantiateWarmUpSpellFX(PlayerManager player)
        {
            base.InstantiateWarmUpSpellFX(player);

            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedWarmUpSpellFX = Instantiate(spellCastWarmUpFX);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }

            instantiatedWarmUpSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedWarmUpSpellFX.transform.localPosition = Vector3.zero;
            instantiatedWarmUpSpellFX.transform.localRotation = Quaternion.identity;

            player.playerEffectsManager.activeSpellWarmUpFX = instantiatedWarmUpSpellFX;
        }

        public override void SuccessfullyCastSpell(PlayerManager player)
        {
            base.SuccessfullyCastSpell(player);

            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            //  INSTANTIATE THE LIGHTNING AOE AT THE CASTER'S POSITION
            GameObject instantiatedLightningFX = Instantiate(spellCastReleaseFX, player.transform.position, Quaternion.identity);

            LightningManager lightningManager = instantiatedLightningFX.GetComponent<LightningManager>();
            lightningManager.InitializeLightning(player, CalculateSpellDamage(player));
        }

        public override void SuccessfullyChargeSpell(PlayerManager player)
        {
            base.SuccessfullyChargeSpell(player);

            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedChargeSpellFX = Instantiate(spellChargeFX);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }

            player.playerEffectsManager.activeSpellWarmUpFX = instantiatedChargeSpellFX;

            instantiatedChargeSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedChargeSpellFX.transform.localPosition = Vector3.zero;
            instantiatedChargeSpellFX.transform.localRotation = Quaternion.identity;
        }

        // ─────────────────────────────────────────────────────────────────
        //  AI Casting Overrides
        // ─────────────────────────────────────────────────────────────────

        public override void AttemptToCastSpell(AICharacterManager aiCharacter)
        {
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
        }

        /// <summary>
        /// 라이트닝은 투사체 방향 없이 시전자 위치에 AOE를 생성합니다.
        /// spellSpawnPoint 는 무시되며, aiCharacter 의 발 위치를 기준으로 스폰됩니다.
        /// </summary>
        public override void SuccessfullyCastSpell(AICharacterManager aiCharacter, Transform spellSpawnPoint, float damage)
        {
            if (spellCastReleaseFX == null)
                return;

            GameObject instantiatedLightningFX = Instantiate(spellCastReleaseFX, aiCharacter.transform.position, Quaternion.identity);

            LightningManager lightningManager = instantiatedLightningFX.GetComponent<LightningManager>();
            if (lightningManager != null)
                lightningManager.InitializeLightning(aiCharacter, damage);
        }

        public override void SuccessfullyCastSpellFullCharge(PlayerManager player)
        {
            base.SuccessfullyCastSpellFullCharge(player);

            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            //  INSTANTIATE THE FULLY CHARGED LIGHTNING AOE AT THE CASTER'S POSITION
            GameObject instantiatedLightningFX = Instantiate(spellCastReleaseFXFullCharge, player.transform.position, Quaternion.identity);

            LightningManager lightningManager = instantiatedLightningFX.GetComponent<LightningManager>();
            lightningManager.isFullyCharged = true;
            lightningManager.InitializeLightning(player, CalculateSpellDamage(player));
        }
    }
}
