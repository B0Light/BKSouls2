using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class WeaponModelInstantiationSlot : MonoBehaviour
    {
        public WeaponModelSlot weaponSlot;
        public GameObject currentWeaponModel;

        public void UnloadWeapon()
        {
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
            }
        }

        public void PlaceWeaponModelIntoSlot(GameObject weaponModel)
        {
            currentWeaponModel = weaponModel;
            weaponModel.transform.parent = transform;

            weaponModel.transform.localPosition = Vector3.zero;
            weaponModel.transform.localRotation = Quaternion.identity;
            weaponModel.transform.localScale = Vector3.one;
        }

        public void PlaceWeaponModelInUnequippedSlot(GameObject weaponModel, WeaponClass weaponClass, PlayerManager player)
        {
            // TO DO, MOVE WEAPON ON BACK CLOSER OR MORE OUTWARD DEPENDING ON CHEST EQUIPMENT (SO IT DOESNT APPEAR TO FLOAT)

            currentWeaponModel = weaponModel;
            weaponModel.transform.parent = transform;

            switch (weaponClass)
            {
                case WeaponClass.StraightSword:
                    weaponModel.transform.localPosition = new Vector3(-0.168f, 0.131f, 0.203f);
                    weaponModel.transform.localRotation = Quaternion.Euler(0.016f, 29.995f, 269.970f);
                    break;
                case WeaponClass.Spear:
                    weaponModel.transform.localPosition = new Vector3(0f, 0.12f, 0.12f);
                    weaponModel.transform.localRotation = Quaternion.Euler(0, 30, -90f);
                    break;
                case WeaponClass.MediumShield:
                    weaponModel.transform.localPosition = new Vector3(0.15f, 0.1f, 0.15f);
                    weaponModel.transform.localRotation = Quaternion.Euler(0, 30, 90f);
                    break;
                default:
                    break;
            }
        }
    }
}
