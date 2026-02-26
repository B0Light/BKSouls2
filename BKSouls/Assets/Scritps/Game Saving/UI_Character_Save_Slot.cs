using System;
using UnityEngine;
using TMPro;

namespace BK
{
    public class UI_Character_Save_Slot : MonoBehaviour
    {
        [Header("Game Slot")]
        public CharacterSlot characterSlot;

        [Header("Character Info")]
        public TextMeshProUGUI characterName;
        public TextMeshProUGUI lastPlayedTime;

        private void OnEnable()
        {
            LoadSaveSlotData();
        }
        
        private void LoadSaveSlotData()
        {
            SaveFileDataWriter saveFileWriter = new SaveFileDataWriter();
            saveFileWriter.saveDataDirectoryPath = Application.persistentDataPath;
    
            saveFileWriter.saveFileName = WorldSaveGameManager.Instance.DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
    
            if (!saveFileWriter.CheckToSeeIfFileExists())
            {
                gameObject.SetActive(false);
                return;
            }

            SetSlotInfo(WorldSaveGameManager.Instance.characterSlots[(int)characterSlot]);
        }
        
        private void SetSlotInfo(CharacterSaveData slot)
        {
            characterName.text = slot.characterName;
        
            lastPlayedTime.text = ConvertPlayTime(slot.lastPlayTime);
        }

        public void LoadGameFromCharacterSlot()
        {
            WorldSaveGameManager.Instance.currentCharacterSlotBeingUsed = characterSlot;
            WorldSaveGameManager.Instance.LoadGame();
        }

        public void SelectCurrentSlot()
        {
            TitleScreenManager.Instance.SelectCharacterSlot(characterSlot);
        }
        
        private string ConvertPlayTime(string isoTime)
        {
            if (DateTime.TryParse(isoTime, out DateTime dateTime))
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                Debug.LogError("Invalid ISO 8601 time format.");
                return "";
            }
        }
    }
}