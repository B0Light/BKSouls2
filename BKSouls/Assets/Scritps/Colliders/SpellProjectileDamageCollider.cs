using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class SpellProjectileDamageCollider : DamageCollider
    {
        public CharacterManager spellCaster;

        protected override void CheckForBlock(CharacterManager damageTarget)
        {
            // Spell damage cannot be guarded.
        }
    }
}
