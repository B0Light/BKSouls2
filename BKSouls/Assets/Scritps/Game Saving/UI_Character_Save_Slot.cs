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
        public TextMeshProUGUI timedPlayed;

        private void OnEnable()
        {
            LoadSaveSlotData();
        }

        private void LoadSaveSlotData()
        {
            // 1. 매니저의 배열에서 해당 슬롯의 데이터를 가져옵니다.
            // Enum을 int로 형변환하여 배열 인덱스로 사용합니다.
            CharacterSaveData slotData = WorldSaveGameManager.instance.allCharacterSlots[(int)characterSlot];

            // 2. 데이터가 존재하는지 확인 (null이 아니면 파일이 있는 것)
            if (slotData != null)
            {
                // 데이터가 있으면 텍스트 UI 업데이트
                characterName.text = slotData.characterName;
                
                // 플레이 시간도 저장되어 있다면 여기에 추가 가능
                timedPlayed.text = slotData.secondsPlayed.ToString(); 
                
                gameObject.SetActive(true);
            }
            else
            {
                // 데이터가 없으면 슬롯 자체를 비활성화 (또는 "Empty" 표시)
                gameObject.SetActive(false);
            }
        }

        public void LoadGameFromCharacterSlot()
        {
            WorldSaveGameManager.instance.currentCharacterSlotBeingUsed = characterSlot;
            WorldSaveGameManager.instance.LoadGame();
        }

        public void SelectCurrentSlot()
        {
            TitleScreenManager.Instance.SelectCharacterSlot(characterSlot);
        }
    }
}