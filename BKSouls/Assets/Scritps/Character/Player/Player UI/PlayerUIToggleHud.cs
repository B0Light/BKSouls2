using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerUIToggleHud : MonoBehaviour
    {
        private void OnEnable()
        {
            //  HIDE THE HUD
            GUIController.Instance.playerUIHudManager.ToggleHUD(false);
        }

        private void OnDisable()
        {
            //  BRING THE HUD BACK
            GUIController.Instance.playerUIHudManager.ToggleHUD(true);
        }
    }
}
