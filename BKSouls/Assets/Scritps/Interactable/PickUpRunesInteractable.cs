using UnityEngine;

namespace BK
{
    public class PickUpRunesInteractable : Interactable
    {
        public int runeCount = 0;

        public override void Interact(PlayerManager player)
        {
            WorldSaveGameManager.Instance.currentCharacterData.hasDeadSpot = false;
            player.playerStatsManager.AddRunes(runeCount);
            Destroy(gameObject);
        }
    }
}
