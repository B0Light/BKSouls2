using UnityEngine;

namespace BK
{
    public class PickUpRunesInteractable : Interactable
    {
        public int runeCount = 0;

        public override void Interact(PlayerManager player)
        {
            WorldSaveGameManager.instance.currentGameData.hasDeadSpot = false;
            player.playerStatsManager.AddRunes(runeCount);
            Destroy(gameObject);
        }
    }
}
