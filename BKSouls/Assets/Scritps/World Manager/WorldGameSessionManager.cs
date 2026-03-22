using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class WorldGameSessionManager : Singleton<WorldGameSessionManager>
    {
        [Header("Active Players In Session")]
        public List<PlayerManager> players = new List<PlayerManager>();

        private Coroutine revivalCoroutien;

        public void WaitThenReviveHost()
        {
            if (revivalCoroutien != null)
                StopCoroutine(revivalCoroutien);

            revivalCoroutien = StartCoroutine(ReviveHostCoroutine(5));
        }

        private IEnumerator ReviveHostCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            GUIController.Instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            GUIController.Instance.localPlayer.ReviveCharacter();

            WorldSceneManager.Instance.LoadWorldScene("Scene_RoundTableHold");
        }

        public void AddPlayerToActivePlayersList(PlayerManager player)
        {
            //  CHECK THE LIST, IF IT DOES NOT ALREADY CONTAIN THE PLAYER, ADD THEM
            if (!players.Contains(player))
            {
                players.Add(player);
            }

            //  CHECK THE LIST FOR NULL SLOTS, AND REMOVE THE NULL SLOTS
            for (int i = players.Count - 1; i > -1; i--)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                }
            }
        }

        public void RemovePlayerFromActivePlayersList(PlayerManager player)
        {
            //  CHECK THE LIST, IF IT DOES CONTAIN THE PLAYER, REMOVE THEM
            if (players.Contains(player))
            {
                players.Remove(player);
            }

            //  CHECK THE LIST FOR NULL SLOTS, AND REMOVE THE NULL SLOTS
            for (int i = players.Count - 1; i > -1; i--)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                }
            }
        }
    }
}
