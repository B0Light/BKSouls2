using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace BK
{
    public class AICharacterNetworkManager : CharacterNetworkManager
    {
        AICharacterManager aiCharacter;

        [Header("Sleep")]
        public NetworkVariable<bool> isAwake = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString64Bytes> sleepingAnimation = new NetworkVariable<FixedString64Bytes>("Sleep_01", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString64Bytes> wakingAnimation = new NetworkVariable<FixedString64Bytes>("Wake_01", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        [Header("Ranged Combat")]
        public NetworkVariable<bool> hasArrowNotched = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);        //  THIS LETS US KNOW IF WE ALREADY HAVE A PROJECTILE LOADED
        public NetworkVariable<bool> isHoldingArrow = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);        //  THIS LETS US KNOW IF WE ARE HOLDING THAT PROJECTILE SO IT DOES NOT RELEASE

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        public override void OnIsDeadChanged(bool oldStatus, bool newStatus)
        {
            base.OnIsDeadChanged(oldStatus, newStatus);

            if (aiCharacter.isDead.Value)
            {
                aiCharacter.aiCharacterInventoryManager.DropItem();
                aiCharacter.aiCharacterCombatManager.AwardRunesOnDeath(GUIController.Instance.localPlayer);
            }
        }

        protected override void PerformReleasedProjectileFromRpc(int projectileID, float xPosition, float yPosition, float zPosition, float yCharacterRotation)
        {
            RangedProjectileItem projectileItem = null;

            //  THE PROJECTILE WE ARE FIRING
            if (WorldItemDatabase.Instance.GetProjectileByID(projectileID) != null)
                projectileItem = WorldItemDatabase.Instance.GetProjectileByID(projectileID);

            if (projectileItem == null)
                return;

            Transform projectileInstantiationLocation;
            GameObject projectileGameObject;
            Rigidbody projectileRigidbody;
            RangedProjectileDamageCollider projectileDamageCollider;

            projectileInstantiationLocation = aiCharacter.aiCharacterCombatManager.lockOnTransform;
            projectileGameObject = Instantiate(projectileItem.releaseProjectileModel, projectileInstantiationLocation);
            projectileDamageCollider = projectileGameObject.GetComponent<RangedProjectileDamageCollider>();
            projectileRigidbody = projectileGameObject.GetComponent<Rigidbody>();

            //  (TODO MAKE FORMULA TO SET RANGE PROJECTILE DAMAGE)
            projectileDamageCollider.physicalDamage = 100;
            projectileDamageCollider.characterShootingProjectile = aiCharacter;

            Quaternion arrowRotation = Quaternion.LookRotation(aiCharacter.aiCharacterCombatManager.currentTarget.characterCombatManager.lockOnTransform.position
                    - projectileGameObject.transform.position);
                projectileGameObject.transform.rotation = arrowRotation;
                
            Collider[] characterColliders = aiCharacter.GetComponentsInChildren<Collider>();
            List<Collider> collidersArrowWillIgnore = new List<Collider>();

            foreach (var item in characterColliders)
                collidersArrowWillIgnore.Add(item);

            foreach (Collider hitBox in collidersArrowWillIgnore)
                Physics.IgnoreCollision(projectileDamageCollider.damageCollider, hitBox, true);

            projectileRigidbody.AddForce(projectileGameObject.transform.forward * projectileItem.forwardVelocity);
            projectileGameObject.transform.parent = null;
        }
    }
}
