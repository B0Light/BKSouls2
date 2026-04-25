using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
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
            WorldSaveGameManager.Instance.RestorePreDungeonStats();

            // 사망 시 보유 룬의 10%를 balance로 환수
            PlayerManager localPlayer = GUIController.Instance.localPlayer;
            if (localPlayer != null)
            {
                int runesOnDeath = localPlayer.playerStatsManager.runes;
                int balanceGain  = Mathf.RoundToInt(runesOnDeath * 0.1f);
                if (balanceGain > 0)
                    WorldPlayerInventory.Instance.balance.Value += balanceGain;
            }

            WorldSaveGameManager.Instance.ResetRunes();
            WorldPlayerInventory.Instance.ClearInventoryAndBackpack();

            // 던전에 보스/적 시신이 남아 있으면 씬 전환 전에 정리
            if (RoomManager.Instance != null)
                RoomManager.Instance.CleanupForSceneTransition();

            // 사망 귀환 시 레벨업 UI 미확정 상태 초기화
            if (GUIController.Instance.playerUILevelUpManager != null)
                GUIController.Instance.playerUILevelUpManager.ResetSliders();

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
