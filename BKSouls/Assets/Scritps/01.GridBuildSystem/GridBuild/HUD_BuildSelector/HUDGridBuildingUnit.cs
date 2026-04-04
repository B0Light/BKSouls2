using BK.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDGridBuildingUnit : MonoBehaviour
{
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TextMeshProUGUI selectNumText;
    [SerializeField] private Button selectButton;

    [Header("Default Exit")] [SerializeField]
    Sprite exitIcon;

    private BuildObjData _buildObjData;

    public void InitButton(int buildingCode, int itemCount)
    {
        _buildObjData = WorldDatabase_Build.Instance.GetBuildingByID(buildingCode);
        buildingIcon.sprite = _buildObjData.itemIcon;
        selectNumText.text = itemCount.ToString();
        selectButton.onClick.AddListener(() => BaseGridBuildSystem.Instance.SelectToBuild(_buildObjData));
    }

    public void InitExitButton()
    {
        buildingIcon.sprite = exitIcon;
        selectNumText.text = "X";
        CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager;
        if (categoryBuildHUDManager)
            selectButton.onClick.AddListener(categoryBuildHUDManager.RefreshCategory);
    }
}
