using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerEffectsManager : CharacterEffectsManager
    {
        [Header("DEBUG DELETE LATER")]
        [SerializeField] bool applyPoisonBuildUp = false;
        [SerializeField] bool applyBleedBuildUp = false;
        [SerializeField] bool applyFrostBuildUp = false;

        protected override void Update()
        {
            base.Update();

            if (applyPoisonBuildUp)
            {
                applyPoisonBuildUp = false;
                TakeBuildUpEffect buildUp = Instantiate(WorldCharacterEffectsManager.Instance.takePoisonBuildUpEffect);
                buildUp.buildUpAmount = 25;
                character.characterEffectsManager.ProcessInstantEffect(buildUp);
            }

            if (applyBleedBuildUp)
            {
                applyBleedBuildUp = false;
                TakeBuildUpEffect buildUp = Instantiate(WorldCharacterEffectsManager.Instance.takeBleedBuildUpEffect);
                buildUp.buildUpAmount = 25;
                character.characterEffectsManager.ProcessInstantEffect(buildUp);
            }

            if (applyFrostBuildUp)
            {
                applyFrostBuildUp = false;
                TakeBuildUpEffect buildUp = Instantiate(WorldCharacterEffectsManager.Instance.takeFrostBuildUpEffect);
                buildUp.buildUpAmount = 25;
                character.characterEffectsManager.ProcessInstantEffect(buildUp);
            }
        }
    }
}