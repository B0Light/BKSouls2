using BK.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_BuildInfo : MonoBehaviour
{
    private BuildObjData _buildData;
    [SerializeField] private TextMeshProUGUI buildName;
    [SerializeField] private TextMeshProUGUI buildLevel;
    [SerializeField] private TextMeshProUGUI buildFee;
    [SerializeField] private TextMeshProUGUI buildDescription;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI upgradeTextCur;
    [SerializeField] private TextMeshProUGUI upgradeTextNext;
    [SerializeField] private TextMeshProUGUI upgradeTextCost;
    
    [SerializeField] private Button upgradeButton;
    
    public void Init(PlacedObject buildTile)
    {
        _buildData = buildTile.GetBuildObjData();
        buildName.text = _buildData.itemName;
        buildLevel.text = "Level : " + buildTile.GetLevel();
        buildFee.text = "Fee : " + buildTile.GetFee();
        buildDescription.text = _buildData.itemDescription;
        
       SetUpgradePanel(buildTile);
    }

    private void SetUpgradePanel(PlacedObject buildTile)
    {
        int curLevel = buildTile.GetLevel();
        int curFee = buildTile.GetFee();
        int nextLevel = curLevel + 1;
        upgradeTextCur.text = "[기존]\n" + GetStateText(curLevel, curFee);
        
        
        if (nextLevel <= buildTile.GetBuildObjData().maxLevel)
        {
            upgradeTextNext.text = "[강화 후]\n" + GetStateText(nextLevel, buildTile.GetFeeByLevel(nextLevel));
            upgradeTextCost.text = "강화 비용 : " + buildTile.GetUpgradeCost() + "stl";
            upgradeButton.interactable = true;
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() =>
            {
                bool upgraded = BaseGridBuildSystem.Instance.TryUpgrade(buildTile);
                if (upgraded)
                {
                    Init(buildTile); // 성공적으로 업그레이드되었을 때만 UI 갱신
                }
                else
                {
                    Debug.LogWarning("Upgrade False");
                }
            });
        }
        else
        {
            upgradeTextNext.text = "[최대 강화]\n" + "최고 레벨 입니다.";
            
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.interactable = false;
        }
    }

    private string GetStateText(int level, int income)
    {
        string unit = "SKL";
        string per = "Time";

        string text = $"레벨 : {level}\n수익 : {income} {unit} / {per}";

        return text;
    }
    
}
