using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerUISiteOfGraceManager : PlayerUIMenu
    {
        public void OpenTeleportLocationMenu()
        {
            CloseMenu();
            PlayerUIManager.instance.playerUITeleportLocationManager.OpenMenu();
        }

        public void OpenLevelUpMenu()
        {
            CloseMenu();
            PlayerUIManager.instance.playerUILevelUpManager.OpenMenu();
        }
    }
}
