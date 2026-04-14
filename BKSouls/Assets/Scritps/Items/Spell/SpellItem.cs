using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class SpellItem : Item
    {
        [Header("Spell Class")]
        public SpellClass SpellClass;

        [Header("Spell Damage")]
        [Tooltip("주문의 기본 데미지. INT 스탯과 장비 무기의 SpellBuff에 의해 최종 데미지가 결정됩니다.")]
        public float baseDamage = 150f;

        [Header("Spell Modifiers")]
        public float fullChargeEffectMultiplier = 2;

        [Header("Spell Costs")]
        public int spellSlotsUsed = 1;
        public int staminaCost = 25;
        public int focusPointCost = 25;

        [Header("Spell FX")]
        [SerializeField] protected GameObject spellCastWarmUpFX;
        [SerializeField] protected GameObject spellChargeFX;
        [SerializeField] protected GameObject spellCastReleaseFX;
        [SerializeField] protected GameObject spellCastReleaseFXFullCharge;
        //  FULL CHARGE VERSION OF FX (TO DO)

        [Header("Animations")]
        [SerializeField] protected string mainHandSpellAnimation;
        [SerializeField] protected string offHandSpellAnimation;

        [Header("Sound FX")]
        public AudioClip warmUpSoundFX;
        public AudioClip releaseSoundFX;

        //  THIS IS WHERE YOU PLAY THE "WARM UP" ANIMATION
        public virtual void AttemptToCastSpell(PlayerManager player)
        {

        }

        //  SPELL FX THAT ARE INSTANTIATED WHEN ATTEMPTING TO CAST THE SPELL
        public virtual void InstantiateWarmUpSpellFX(PlayerManager player)
        {

        }

        //  THIS IS WHERE SPELL PROJECTS/FX ARE ACTIVATED
        public virtual void SuccessfullyCastSpell(PlayerManager player)
        {
            if (player.IsOwner)
            {
                player.playerNetworkManager.currentFocusPoints.Value -= focusPointCost;
                player.playerNetworkManager.currentStamina.Value -= staminaCost;
            }
        }

        public virtual void SuccessfullyChargeSpell(PlayerManager player)
        {

        }

        public virtual void SuccessfullyCastSpellFullCharge(PlayerManager player)
        {
            if (player.IsOwner)
            {
                player.playerNetworkManager.currentFocusPoints.Value -= Mathf.RoundToInt(focusPointCost * fullChargeEffectMultiplier);
                player.playerNetworkManager.currentStamina.Value -= staminaCost * fullChargeEffectMultiplier; ;
            }
        }

        // 최종 주문 데미지 계산: baseDamage × 무기 SpellBuff × INT 스케일링 × 요구스탯 페널티
        protected float CalculateSpellDamage(PlayerManager player)
        {
            var net = player.playerNetworkManager;

            // INT 스탯 스케일링 (WeaponManager.StatScalingBonus와 동일한 곡선)
            int intel = net.intelligence.Value;
            float normalized = Mathf.Clamp01(intel / 99f);
            float intMultiplier = 1f + (1f - Mathf.Pow(1f - normalized, 2f)) * 0.5f;
            // INT 1 → ×1.01, INT 20 → ×1.20, INT 40 → ×1.33, INT 60 → ×1.42, INT 99 → ×1.50

            // 장비 중인 CasterWeaponItem 검색 (오른손 우선)
            float spellBuff = 100f;
            CasterWeaponItem casterWeapon = player.playerInventoryManager.currentRightHandWeapon as CasterWeaponItem;
            if (casterWeapon == null)
                casterWeapon = player.playerInventoryManager.currentLeftHandWeapon as CasterWeaponItem;
            if (casterWeapon != null)
                spellBuff = casterWeapon.spellBuff;

            // 요구 스탯 미충족 시 데미지 절반
            float requirementPenalty = 1f;
            if (casterWeapon != null)
            {
                int str   = net.strength.Value + net.strengthModifier.Value;
                int dex   = net.dexterity.Value;
                int faith = net.faith.Value;
                bool meets = str   >= casterWeapon.strengthREQ
                          && dex   >= casterWeapon.dexREQ
                          && intel >= casterWeapon.intREQ
                          && faith >= casterWeapon.faithREQ;
                if (!meets)
                    requirementPenalty = 0.5f;
            }

            return baseDamage * (spellBuff / 100f) * intMultiplier * requirementPenalty;
        }

        //  HELPER FUNCTION TO CHECK WEATHER OR NOT WE ARE ABLE TO USE THE SPELL WHEN ATTEMPTING TO CAST
        public virtual bool CanICastThisSpell(PlayerManager player)
        {
            if (player.playerNetworkManager.currentFocusPoints.Value <= focusPointCost)
                return false;

            if (player.playerNetworkManager.currentStamina.Value <= staminaCost)
                return false;

            if (player.isPerformingAction)
                return false;

            if (player.playerNetworkManager.isJumping.Value)
                return false;

            return true;
        }
    }
}
