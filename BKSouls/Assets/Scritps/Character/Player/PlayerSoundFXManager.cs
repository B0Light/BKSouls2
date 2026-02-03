using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerSoundFXManager : CharacterSoundFXManager
    {
        PlayerManager player;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public override void PlayBlockSoundFX()
        {
            PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(player.playerCombatManager.currentWeaponBeingUsed.blocking));
        }

        public override void PlayFootStepSoundFX()
        {
            base.PlayFootStepSoundFX();

            WorldSoundFXManager.Instance.AlertNearbyCharactersToSound(transform.position, 2);
        }
    }
}
