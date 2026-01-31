using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace BK.Inventory
{
    public class ShredderHUDManager : MonoBehaviour
    {
        [SerializeField] private ItemGrid itemGrid;
        [SerializeField] [Range(0f, 1f)] private float disassemblyRate;
        [SerializeField] private float resetTimer = 30f;
        private int _disassembledValue;
        private bool _isDisassemble = false;
        private float _currentResetTime;

        [Header("UI")] [SerializeField] private TextMeshProUGUI totalItemCostText;
        [SerializeField] private Button disassembleButton;
        private List<int> _initItemList;

        public void Init(int width, int height, List<int> itemIdList)
        {
            disassembleButton.onClick.AddListener(DisassembleItems);
            //WorldSceneChangeManager.OnSceneEndPhase += ResetDisassemble;

            _initItemList = itemIdList;
            itemGrid.SetGrid(width, height, itemIdList);
        }

        private void FixedUpdate()
        {
            if (_isDisassemble)
            {
                _currentResetTime -= Time.fixedDeltaTime;

                if (_currentResetTime <= 0)
                {
                    ResetDisassemble();
                    return;
                }

                int remainingTime = Mathf.CeilToInt(_currentResetTime);
                totalItemCostText.text = $"Disassembly complete. Crusher reactivating in {remainingTime}s";
                return;
            }

            totalItemCostText.text = $"Value : {CalculateDisassembledValue()}";
        }

        private int CalculateDisassembledValue()
        {
            _disassembledValue = (int)(itemGrid.totalItemValue.Value * disassemblyRate);
            return _disassembledValue;
        }

        private void DisassembleItems()
        {
            _isDisassemble = true;
            disassembleButton.interactable = false;

            totalItemCostText.text = "Disassembly complete. Crusher is now inactive.";
            WorldPlayerInventory.Instance.balance.Value += _disassembledValue;
            _disassembledValue = 0;
            itemGrid.ResetItemGrid();
            _initItemList.Clear();

            // 리셋 타이머 시작
            _currentResetTime = resetTimer;
        }

        private void ResetDisassemble()
        {
            _isDisassemble = false;
            disassembleButton.interactable = true;
            _currentResetTime = 0f;
        }

        public ItemGrid GetItemGrid => itemGrid;
    }
}