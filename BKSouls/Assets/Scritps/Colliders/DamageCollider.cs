using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK {
    public class DamageCollider : MonoBehaviour
    {
        [Header("Collider")]
        public Collider damageCollider;

        [Header("Damage")]
        public float physicalDamage = 0;            // (TO DO, SPLIT INTO "Standard", "Strike", "Slash" and "Pierce")
        public float magicDamage = 0;
        public float fireDamage = 0;
        public float lightningDamage = 0;
        public float holyDamage = 0;

        [Header("Poise")]
        public float poiseDamage = 0;

        [Header("Contact Point")]
        protected Vector3 contactPoint;

        [Header("Characters Damaged")]
        protected List<CharacterManager> charactersDamaged = new List<CharacterManager>();

        [Header("Block")]
        protected Vector3 directionFromAttackToDamageTarget;
        protected float dotValueFromAttackToDamageTarget;

        protected virtual void Awake()
        {

        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget != null)
            {
                contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

                //  CHECK IF TARGET IS BLOCKING
                CheckForBlock(damageTarget);

                //  CHECK IF TARGET IS PARRYING
                CheckForParry(damageTarget);

                if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                    DamageTarget(damageTarget);
            }
        }

        protected virtual void CheckForBlock(CharacterManager damageTarget)
        {
            //  IF THIS CHARACTER HAS ALREADY BEEN DAMAGED, DO NOT PROCEED
            if (charactersDamaged.Contains(damageTarget))
                return;

            GetBlockingDotValues(damageTarget);

            // 1. CHECK IF THE CHARACTER BEING DAMAGED IS BLOCKING
            if (damageTarget.characterNetworkManager.isBlocking.Value && dotValueFromAttackToDamageTarget > 0.3f)
            {
                // 2. IF THE CHARACTER IS BLOCKING, CHECK IF THEY ARE FACING IN THE CORRECT DIRECTION TO BLOCK SUCCESSFULLY

                charactersDamaged.Add(damageTarget);

                TakeBlockedDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);

                damageEffect.physicalDamage = physicalDamage;
                damageEffect.magicDamage = magicDamage;
                damageEffect.fireDamage = fireDamage;
                damageEffect.holyDamage = holyDamage;
                damageEffect.poiseDamage = poiseDamage;
                damageEffect.staminaDamage = poiseDamage;   // IF YOU WANT TO GIVE STAMINA DAMAGE ITS OWN VARIABLE, INSTEAD OF USING POISE GO FOR IT
                damageEffect.contactPoint = contactPoint;

                // 3. APPLY BLOCKED CHARACTER DAMAGE TO TARGET
                damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
            }
        }

        protected virtual void CheckForParry(CharacterManager damageTarget)
        {

        }

        protected virtual void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = transform.position - damageTarget.transform.position;
            dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
        }

        protected virtual void DamageTarget(CharacterManager damageTarget)
        {
            //  WE DON'T WANT TO DAMAGE THE SAME TARGET MORE THAN ONCE IN A SINGLE ATTACK
            //  SO WE ADD THEM TO A LIST THAT CHECKS BEFORE APPLYING DAMAGE
            if (charactersDamaged.Contains(damageTarget))
                return;

            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicDamage = magicDamage;
            damageEffect.fireDamage = fireDamage;
            damageEffect.holyDamage = holyDamage;
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
        }

        public virtual void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        public virtual void DisableDamageCollider()
        {
            damageCollider.enabled = false;
            charactersDamaged.Clear();      //  WE RESET THE CHARACTERS THAT HAVE BEEN HIT WHEN WE RESET THE COLLIDER, SO THEY MAY BE HIT AGAIN
        }
    }
}
