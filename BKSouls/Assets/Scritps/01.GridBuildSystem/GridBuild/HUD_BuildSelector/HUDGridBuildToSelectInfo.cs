using System;
using TMPro;
using UnityEngine;
using System.Collections;
using BK.Inventory;

public class HUDGridBuildToSelectInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buildName;
    [SerializeField] private TextMeshProUGUI buildInfo;
    [SerializeField] private GridBuildingUI gridBuildingUI;

    [SerializeField] private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if(_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
        while (!BaseGridBuildSystem.Instance)
        {
            yield return null; // 한 프레임 대기
        }
    }
    
    private void OnEnable()
    {
        StartCoroutine(BindSelectBuilding());
    }

    private void OnDisable()
    {
        BaseGridBuildSystem.OnSelectedChanged -= Instance_OnSelectedChanged;
    }

    private IEnumerator BindSelectBuilding()
    {
        yield return StartCoroutine(WaitForDataLoad());
        BaseGridBuildSystem.OnSelectedChanged += Instance_OnSelectedChanged;
    }
    
    private void Instance_OnSelectedChanged(BuildObjData objectToPlace)
    {
        BuildObjData buildObjData = objectToPlace;
        _canvasGroup.alpha = buildObjData != null ? 1 : 0;
        Init(buildObjData);
    }

    public void Show(BuildObjData buildData)
    {
        Init(buildData);
        SetVisible(true);
    }

    public void Hide()
    {
        Init(null);
        SetVisible(false);
    }

    private void SetVisible(bool isActive)
    {
        _canvasGroup.alpha = isActive ? 1f : 0f;
        _canvasGroup.interactable = isActive;
        _canvasGroup.blocksRaycasts = isActive;
    }

    private void Init(BuildObjData buildData)
    {
        if (buildData)
        {
            buildName.text = buildData.itemName;
            buildInfo.text = GetInfoText(buildData);

            if (gridBuildingUI != null)
            {
                PlacedObject placedObject = buildData.prefab != null
                    ? buildData.prefab.gameObject.GetComponent<PlacedObject>()
                    : null;

                if (placedObject != null)
                    gridBuildingUI.SetGridLayer(buildData.width, buildData.height, placedObject.entrancePos, placedObject.exitDir, placedObject.exitPos);
                else
                    gridBuildingUI.SetGridLayer(buildData.width, buildData.height);
            }
        }
        else
        {
            buildName.text = "";
            buildInfo.text = "";
            if (gridBuildingUI != null)
                gridBuildingUI.ClearGrid();
        }
    }

    private string GetInfoText(BuildObjData buildData)
    {
        string description = buildData.itemDescription;
        
        string text = $"{description}";

        return text;
    }
}
