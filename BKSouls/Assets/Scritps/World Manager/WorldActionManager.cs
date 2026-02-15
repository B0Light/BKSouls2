using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BK
{
    public class WorldActionManager : Singleton<WorldActionManager>
    {
        [Header("Weapon Item Actions")]
        public WeaponItemAction[] weaponItemActions;
        

        private void Start()
        {
            for (int i = 0; i < weaponItemActions.Length; i++)
            {
                weaponItemActions[i].actionID = i;
            }
        }

        public WeaponItemAction GetWeaponItemActionByID(int ID)
        {
            return weaponItemActions.FirstOrDefault(action => action.actionID == ID);
        }
    }
}
