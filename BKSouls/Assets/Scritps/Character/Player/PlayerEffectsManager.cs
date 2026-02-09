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

        protected override void Update()
        {
            base.Update();  
            if (applyPoisonBuildUp)
            {
                applyPoisonBuildUp = false;
                TakeBuildUpEffect buildUp = Instantiate(WorldCharacterEffectsManager.instance.takePoisonBuildUpEffect);
                buildUp.buildUpAmount = 25;
                character.characterEffectsManager.ProcessInstantEffect(buildUp);
            }

            if (applyBleedBuildUp)
            {
                applyBleedBuildUp = false;
                TakeBuildUpEffect buildUp = Instantiate(WorldCharacterEffectsManager.instance.takeBleedBuildUpEffect);
                buildUp.buildUpAmount = 25;
                character.characterEffectsManager.ProcessInstantEffect(buildUp);
            }
        }
    }
}
