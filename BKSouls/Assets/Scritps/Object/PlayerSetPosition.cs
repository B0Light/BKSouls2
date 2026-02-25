using Unity.Netcode;
using UnityEngine;

namespace BK
{


    public class PlayerSetPosition : MonoBehaviour
    {
        void Start()
        {

        }

        private void SetPosition()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  ENABLE LOADING SCREEN
            GUIController.Instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            //  TELEPORT PLAYER
            player.transform.position = this.transform.position;

            //  DISABLE LOADING SCREEN
            GUIController.Instance.playerUILoadingScreenManager.DeactivateLoadingScreen(1);
        }
    }
}