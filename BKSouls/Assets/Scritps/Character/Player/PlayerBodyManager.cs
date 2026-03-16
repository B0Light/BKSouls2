using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerBodyManager : MonoBehaviour
    {
        PlayerManager player;
        
        [Header("Hair Object")]
        [SerializeField] public GameObject hair;
        [SerializeField] private GameObject[] hairObjects;

        private void Awake()
        {
            player = GetComponent<PlayerManager>();
        }

        
        public void EnableHead()
        {
            
        }

        public void DisableHead()
        {
            
        }

        public void EnableHair()
        {
            
        }

        public void DisableHair()
        {
            
        }

        public void ToggleHairType(int hairType)
        {
            //  DISABLE ALL HAIR
            for (int i = 0; i < hairObjects.Length; i++)
            {
                hairObjects[i].SetActive(false);
            }

            //  ENABLE CHOOSEN HAIR
            hairObjects[hairType].SetActive(true);
        }

        public void SetHairColor()
        {
            // 1. IF YOU ARE USING A REGULAR MATERIAL AS A HAIR MATERIAL, SIMPLY CHANGE ITS COLOR
            //  IF YOU ARE USING A MATERIAL WITH MULITPLE COLOR VARIABLES, SIMPLY SET THE CORRECT COLOR

            Color32 hairColor;

            byte red = (byte)player.playerNetworkManager.hairColorRed.Value;
            byte green = (byte)player.playerNetworkManager.hairColorGreen.Value;
            byte blue = (byte)player.playerNetworkManager.hairColorBlue.Value;

            hairColor = new Color32(red, green, blue, 255);

            for (int i = 0; i < hairObjects.Length; i++)
            {
                SkinnedMeshRenderer skinMeshRenderer = hairObjects[i].GetComponent<SkinnedMeshRenderer>();

                if (skinMeshRenderer != null)
                    skinMeshRenderer.material.SetColor("_Color_Hair", hairColor);
            }
        }
    }
}
