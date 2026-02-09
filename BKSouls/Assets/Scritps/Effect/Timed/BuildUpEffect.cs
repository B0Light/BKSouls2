using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Character Effects/Timed Effects/Build Up Effect")]
    public class BuildUpEffect : TimedCharacterEffect
    {
        [Header("Type")]
        public BuildUp buildUpType;

        [Header("Degradation")]
        public int buildUpAmountDegradation = -1;
        public float buildUpRemaining = 1;

        public override void ProcessEffect(CharacterManager character)
        {
            if (!character.IsOwner)
                return;

            //  IF THE BUILD UP FADES OUT, OR REACHES IT'S CLIMAX REMOVE THIS TIMED EFFECT
            if (buildUpRemaining < 0 || buildUpRemaining >= character.characterStatsManager.CalculateBuildUpCapacityBasedOnVitalityLevel(character.characterNetworkManager.vigor.Value))
                character.characterEffectsManager.RemoveTimedEffect(effectID);

            DegradeBuildUp(character);
        }

        public override void RemoveEffect(CharacterManager character)
        {
            base.RemoveEffect(character);
        }

        private void DegradeBuildUp(CharacterManager character)
        {
            character.characterStatsManager.DegradeBuildUps(buildUpType, buildUpAmountDegradation, this);
        }
    }
}