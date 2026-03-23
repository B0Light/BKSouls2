using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// 원거리 공격 AI의 전투 매니저.
    /// - 투사체 발사 지점과 프리팹을 인스펙터에서 설정한다.
    /// - 애니메이션 이벤트에서 FireProjectile() 을 호출하면 타겟을 향해 투사체를 발사한다.
    /// </summary>
    public class AIRangedCharacterCombatManager : AICharacterCombatManager
    {
        [Header("Bow Settings")]
        [SerializeField] private Animator bowAnimator;
        [SerializeField] private Transform arrowSpawnPoint;

        [Header("Projectile Settings")]
        [SerializeField] private RangedProjectileItem projectileItem;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float projectileUpwardVelocity = 0.5f;

        // ─────────────────────────────────────────────────────────
        //  애니메이션 이벤트에서 호출
        // ─────────────────────────────────────────────────────────

        public void DrawBow()
        {
            if (aiCharacter.IsOwner)
                aiCharacter.aiCharacterNetworkManager.hasArrowNotched.Value = true;

            //  PLAY DRAW BOW SFX
            aiCharacter.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(WorldSoundFXManager.Instance.notchArrowSFX));

            GameObject arrow = Instantiate(projectileItem.drawProjectileModel, arrowSpawnPoint);
            aiCharacter.characterEffectsManager.activeDrawnProjectileFX = arrow;

            //  ANIMATE THE BOW
            bowAnimator.SetBool("isDrawn", true);
            bowAnimator.Play("Bow_Draw_01");
        }

        /// <summary>
        /// 애니메이션 이벤트로 호출됩니다.
        /// 현재 타겟을 향해 투사체를 발사합니다.
        /// </summary>
        public void ReleaseArrow()
        {
            if (aiCharacter.IsOwner)
                aiCharacter.aiCharacterNetworkManager.hasArrowNotched.Value = false;

            //  DESTROY THE "WARM UP" PROJECTILE
            if (aiCharacter.characterEffectsManager.activeDrawnProjectileFX != null)
                Destroy(aiCharacter.characterEffectsManager.activeDrawnProjectileFX);

            //  PLAY RELEASE ARROW SFX
            aiCharacter.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(WorldSoundFXManager.Instance.releaseArrowSFX));

            //  ANIMATE THE BOW
            bowAnimator.SetBool("isDrawn", false);
            bowAnimator.Play("Bow_Fire_01");

            if (!aiCharacter.IsOwner)
                return;

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
            projectileDamageCollider.physicalDamage = baseDamage;
            projectileDamageCollider.poiseDamage = basePoiseDamage;
            projectileDamageCollider.characterShootingProjectile = aiCharacter;

            //  FIRE AN ARROW BASED ON 1 OF 3 VARIATIONS

            float yRotationDuringFire = aiCharacter.transform.localEulerAngles.y;

            // AIMING

            // LOCKED AND NOT AIMING
            Quaternion arrowRotation = Quaternion.LookRotation(aiCharacter.aiCharacterCombatManager.currentTarget.characterCombatManager.lockOnTransform.position
                    - projectileGameObject.transform.position);
            projectileGameObject.transform.rotation = arrowRotation;

            //  GET ALL CHARACTER COLLIDERS AND IGNORE SELF
            Collider[] characterColliders = aiCharacter.GetComponentsInChildren<Collider>();
            List<Collider> collidersArrowWillIgnore = new List<Collider>();

            foreach (var item in characterColliders)
                collidersArrowWillIgnore.Add(item);

            foreach (Collider hitBox in collidersArrowWillIgnore)
                Physics.IgnoreCollision(projectileDamageCollider.damageCollider, hitBox, true);

            projectileRigidbody.AddForce(projectileGameObject.transform.forward * projectileItem.forwardVelocity);
            projectileGameObject.transform.parent = null;

            aiCharacter.aiCharacterNetworkManager.NotifyServerOfReleasedProjectileServerRpc(
                aiCharacter.OwnerClientId, 
                projectileItem.itemID, 
                0, 0, 0, yRotationDuringFire); // 사용하지 않는 필드 
        }
    }
}
