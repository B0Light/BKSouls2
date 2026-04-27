using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerAnimatorManager : CharacterAnimatorManager
    {
        PlayerManager player;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        private void OnAnimatorMove()
        {
            if (!applyRootMotion)
                return;

            if (player == null || player.animator == null || player.characterController == null)
                return;

            if (!player.characterController.enabled || !player.characterController.gameObject.activeInHierarchy)
                return;

            Vector3 velocity = player.animator.deltaPosition;
            player.characterController.Move(velocity);
            player.transform.rotation *= player.animator.deltaRotation;
        }

    }
}
