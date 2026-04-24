using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "A.I/Actions/Mage Spell")]
    public class AIMageSpellAction : AICharacterAttackAction
    {
        [Header("Spell")]
        [SerializeField] private SpellItem spell;

        public override void AttemptToPerformAction(AICharacterManager aiCharacter)
        {
            if (spell == null)
                return;

            AIMageCombatManager mageCombat = aiCharacter.aiCharacterCombatManager as AIMageCombatManager;
            if (mageCombat == null)
                return;

            mageCombat.currentSpell = spell;
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(attackAnimation, true);
            aiCharacter.aiCharacterNetworkManager.isParryable.Value = false;
        }
    }
}
