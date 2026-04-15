using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerUISiteOfGraceManager : PlayerUIMenu
    {
        public void OpenLevelUpMenu()
        {
            CloseMenu();
            GUIController.Instance.playerUILevelUpManager.OpenMenu();
        }
    }
}
