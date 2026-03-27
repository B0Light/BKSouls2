using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class EventTriggerBossFight : MonoBehaviour
    {
        [SerializeField] int bossID;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                TriggerBossFight();
            }
        }

        private void TriggerBossFight()
        {
            AIBossCharacterManager boss = FindAnyObjectByType<AIBossCharacterManager>();

            if (boss != null && boss.isDead.Value == false)
            {
                boss.WakeBoss();
            }
        }
    }
}
