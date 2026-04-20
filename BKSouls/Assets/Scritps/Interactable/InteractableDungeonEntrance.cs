using UnityEngine;

namespace BK
{

    public class InteractableDungeonEntrance : Interactable
    {
        [SerializeField] private DungeonInfoData dungeonInfoData;
        [SerializeField] private ParticleSystem vfx;
        [SerializeField] private GameObject vCam;

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);
            WorldSaveGameManager.Instance.SaveGame();
            WorldSaveGameManager.Instance.SnapshotPreDungeonStats();

            vCam?.SetActive(true);
            vfx?.Play();
            OpenHUD();
        }

        private void OpenHUD()
        {
            GUIController.Instance.OpenDungeonEntrance(dungeonInfoData, this);
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();
            vCam?.SetActive(false);
            vfx?.Stop();
        }
    }
}
