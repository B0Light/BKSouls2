using BK.Inventory;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BK
{
    public class InteractableDungeonEntrance : Interactable
    {
        [SerializeField] private DungeonData dungeonData;
        [SerializeField] private ParticleSystem vfx;
        [SerializeField] private GameObject vCam;

        public override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);
            if (vfx != null) vfx.Play();
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);
            if (vfx != null) vfx.Stop();
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only host/server can enter dungeon.");
                return;
            }

            WorldSaveGameManager.Instance.SaveGame();
            WorldSaveGameManager.Instance.SnapshotPreDungeonStats();
            WorldSaveGameManager.Instance.ResetRunes();

            player.playerInteractionManager?.SuppressInteractionsUntilSceneChange();
            GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
            if (vfx != null) vfx.Stop();
            if (vCam != null) vCam.SetActive(true);

            WorldPlayerInventory.Instance.MoveInventoryToShare();
            NetworkManager.Singleton.SceneManager.LoadScene(dungeonData.dungeonSceneName, LoadSceneMode.Single);
        }

        public override void ResetInteraction()
        {
            base.ResetInteraction();
            if (vCam != null) vCam.SetActive(false);
        }
    }
}
