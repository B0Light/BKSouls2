using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Build Up Effect")]
    public class TakeBuildUpEffect : InstantCharacterEffect
    {
        [Header("Build Up")]
        [SerializeField] BuildUp buildUpType;
        public int buildUpAmount = 10;

        public override void ProcessEffect(CharacterManager character)
        {
            base.ProcessEffect(character);

            //  1. ADD BUILD UP TO THE AFFECTED CHARACTER
            character.characterEffectsManager.AddBuildUps(buildUpType, buildUpAmount);

            //  2. CHECK IF THE BUILD UP HAS REACHED ITS LIMIT
            switch (buildUpType)
            {
                case BuildUp.Poison:
                    CheckForPoisonedStatus(character);
                    break;
                case BuildUp.Bleed:
                    CheckForBloodLossStatus(character);
                    break;
                default:
                    break;
            }
        }


        //  3. IF THE BUILD UP HAS REACHED ITS LIMIT APPLY A SPECIAL EFFECT (POISONED, BLOOD LOSS ECT)
        private void CheckForPoisonedStatus(CharacterManager character)
        {
            // 1. IF THE CHARACTER IS ALREADY POISONED, SIMPLY RETURN
            if (character.characterNetworkManager.isPoisoned.Value)
                return;
            
            // 2. CHECK FOR A "TIMED" BUILD UP EFFECT OF TYPE POISON (WE ADD A TIMED BUILD UP EFFECT SO THE BUILD UP CAN DECAY OVER TIME)
            BuildUpEffect poisonBuildUp = character.characterEffectsManager.CheckForTimedEffect(WorldCharacterEffectsManager.instance.degradePoisonBuildUpEffect.effectID) as BuildUpEffect;

            // 3. IF THAT EFFECT DOES NOT EXIST MAKE ONE AND APPLY IT
            if (poisonBuildUp == null)
            {
                poisonBuildUp = Instantiate(WorldCharacterEffectsManager.instance.degradePoisonBuildUpEffect);
                character.characterEffectsManager.AddTimedEffect(poisonBuildUp);
                poisonBuildUp.ProcessEffect(character);
            }


            // 4. IF THE CHARACTER IS OVER THEIR BUILD UP LIMIT, APPLY THE NEW TIMED EFFECT "POISONED"
            if (character.characterNetworkManager.poisonBuildUp.Value > character.characterNetworkManager.buildUpCapacity.Value)
            {
                //  RESET THE BUILD UP AMOUNT, AND SET THE STATUS EFFECT FLAG TO TRUE
                character.characterNetworkManager.poisonBuildUp.Value = 0;
                character.characterNetworkManager.isPoisoned.Value = true;

                //  CREATE THE POISONED EFFECT
                PoisonedEffect poison = Instantiate(WorldCharacterEffectsManager.instance.poisonedEffect);
                character.characterEffectsManager.AddTimedEffect(poison);

                PlayerManager player = character as PlayerManager;

                if (player == null)
                    return;

                //  IF YOU ARE SYNCING THESE EFFECTS VIA RPC CALLS DO AN OWNER CHECK HERE
                if (!player.IsOwner)
                    return;

                //  TO DO ADD A TEMPORARY RESISTANCE SO YOU CAN'T GET POISONED BACK TO BACK SO EASILY
            }
        }

        private void CheckForBloodLossStatus(CharacterManager character)
        {

        }
    }
}
