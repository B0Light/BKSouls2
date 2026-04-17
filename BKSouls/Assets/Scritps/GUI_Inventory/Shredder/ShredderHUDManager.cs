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
        private int _disassembledValue;

        [Header("UI")] [SerializeField] private TextMeshProUGUI totalItemCostText;
        [SerializeField] private Button disassembleButton;
        private List<int> _initItemList;

        public void Init(int width, int height, List<int> itemIdList)
        {
            disassembleButton.onClick.AddListener(DisassembleItems);

            _initItemList = itemIdList;
            itemGrid.SetGrid(width, height, itemIdList);
        }

        private void FixedUpdate()
        {
            totalItemCostText.text = $"Value : {CalculateDisassembledValue()}";
        }

        private int CalculateDisassembledValue()
        {
            _disassembledValue = (int)(itemGrid.totalItemValue.Value * disassemblyRate);
            return _disassembledValue;
        }

        private void DisassembleItems()
        {
            totalItemCostText.text = "Disassembly complete.";
            WorldPlayerInventory.Instance.balance.Value += _disassembledValue;
            _disassembledValue = 0;
            itemGrid.ResetItemGrid();
            _initItemList.Clear();
        }

        public ItemGrid GetItemGrid => itemGrid;
    }
}
