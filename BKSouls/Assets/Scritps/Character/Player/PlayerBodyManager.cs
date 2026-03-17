using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class PlayerBodyManager : MonoBehaviour
    {
        PlayerManager player;
        
        [Header("Hair Object")]
        [SerializeField] private GameObject[] hairObjects;

        [Header("Body Object")] 
        [SerializeField] private GameObject[] bodyObjects;

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
            for (int i = 0; i < hairObjects.Length; i++)
            {
                hairObjects[i].SetActive(false);
            }

            hairObjects[hairType].SetActive(true);
        }

        public void SetHairColor()
        {
            
        }
    }
}
