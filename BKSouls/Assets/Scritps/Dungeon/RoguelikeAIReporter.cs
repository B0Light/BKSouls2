using UnityEngine;

namespace BK
{
    public class RoguelikeAIReporter : MonoBehaviour
    {
        private RoomManager roomManager;
        private AICharacterManager aiCharacter;
        private bool notifiedDeath;

        public void Initialize(RoomManager ownerRoomManager, AICharacterManager ownerAI)
        {
            roomManager = ownerRoomManager;
            aiCharacter = ownerAI;
            notifiedDeath = false;
        }

        private void Update()
        {
            if (roomManager == null || aiCharacter == null || notifiedDeath)
                return;

            if (aiCharacter.isDead.Value)
            {
                notifiedDeath = true;
                roomManager.NotifyEnemyDead(aiCharacter);
            }
        }
    }
}