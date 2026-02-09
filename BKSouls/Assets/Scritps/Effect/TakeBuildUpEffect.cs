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

            // 2. CHECK FOR A "TIMED" BUILD UP EFFECT OF TYPE POISON (WE ADD A TIMED BUILD UP EFFECT SO THE BUILD UP CAN DECAY OVER TIME)

            // 3. IF THAT EFFECT DOES NOT EXIST MAKE ONE AND APPLY IT

            // 4. IF THE CHARACTER IS OVER THEIR BUILD UP LIMIT, APPLY THE NEW TIMED EFFECT "POISONED"
        }

        private void CheckForBloodLossStatus(CharacterManager character)
        {

        }
    }
}
