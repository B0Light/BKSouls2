using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using Unity.Netcode;
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

            PlayerManager localPlayer = GUIController.Instance.localPlayer;
            int runesOnDeath = localPlayer != null ? localPlayer.playerStatsManager.GetRewardableRunes() : 0;
            int balanceGain = Mathf.RoundToInt(runesOnDeath * 0.1f);
            int roomsCleared = RunManager.Instance != null ? Mathf.Max(0, RunManager.Instance.CurrentRoomIndex) : 0;
            int playerLevel = localPlayer != null ? localPlayer.characterStatsManager.CalculateCharacterLevelBasedOnAttributes() : 0;
            int runesSpent = localPlayer != null ? localPlayer.playerStatsManager.runesSpentThisDungeon : 0;
            DungeonResultData resultData = new DungeonResultData(false, roomsCleared, balanceGain, playerLevel, runesSpent);

            DungeonResultUIManager resultUI = GUIController.Instance != null
                ? GUIController.Instance.dungeonResultUIManager
                : null;

            if (resultUI != null)
            {
                GUIController.Instance.CloseGUI();
                resultUI.Open(resultData, () => ReturnToShelterAfterDungeonFailure(balanceGain));
                yield break;
            }

            ReturnToShelterAfterDungeonFailure(balanceGain);
        }

        private void ReturnToShelterAfterDungeonFailure(int balanceGain)
        {
            GUIController.Instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            GUIController.Instance.localPlayer.ReviveCharacter();
            WorldSaveGameManager.Instance.ResetStatsForShelterReturn();

            if (GUIController.Instance.localPlayer != null && balanceGain > 0)
                WorldPlayerInventory.Instance.balance.Value += balanceGain;

            WorldSaveGameManager.Instance.ResetRunes();
            WorldPlayerInventory.Instance.ClearInventoryAndBackpack();
            WorldPlayerInventory.Instance.ClearEquipmentSlots();

            if (GUIController.Instance.playerUILevelUpManager != null)
                GUIController.Instance.playerUILevelUpManager.ResetSliders();

            bool isClientOnly = NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsClient
                && !NetworkManager.Singleton.IsHost;

            if (isClientOnly)
            {
                if (RunManager.Instance != null && RunManager.Instance.IsSpawned)
                    RunManager.Instance.RequestReturnToShelterServerRpc();
            }
            else
            {
                if (RoomManager.Instance != null)
                    RoomManager.Instance.CleanupForSceneTransition();

                WorldSceneManager.Instance.LoadWorldScene("Scene_RoundTableHold");
            }
        }

        public void AddPlayerToActivePlayersList(PlayerManager player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }

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
            if (players.Contains(player))
            {
                players.Remove(player);
            }

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
