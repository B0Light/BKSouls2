using System;
using System.Collections;
using BK.Inventory;
using UnityEngine;

public class BuildingGhost : MonoBehaviour
{
    [SerializeField] private GridBuildController gridBuildController;
    
    [SerializeField] private Material ghostMaterialEnable;
    [SerializeField] private Material ghostMaterialDisable;
    private Transform _visual;
    private Material _curOriginMat;
    private Material _curMat;

    private void Start() 
    {
        RefreshVisual(null);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForGridBuildingSystem());
    }
    
    private IEnumerator WaitForGridBuildingSystem()
    {
        // GridBuildingSystem.Instance가 null이 아닌지 확인
        while (BaseGridBuildSystem.Instance == null)
        {
            yield return null; // 매 프레임 기다림
        }

        // GridBuildingSystem.Instance가 설정되었을 때 이벤트 등록
        BaseGridBuildSystem.OnObjectPlaced += Instance_OnSelectedChanged;
        BaseGridBuildSystem.OnSelectedChanged += Instance_OnSelectedChanged;
    }

    private void OnDisable()
    {
        BaseGridBuildSystem.OnObjectPlaced -= Instance_OnSelectedChanged;
        BaseGridBuildSystem.OnSelectedChanged -= Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(BuildObjData buildObjData) 
    {
        RefreshVisual(buildObjData);
    }

    private void LateUpdate() 
    {
        if(BaseGridBuildSystem.Instance.ObjectToPlace == null) return;
        Vector3 targetPosition = gridBuildController.GetMouseWorldSnappedPosition();
        targetPosition.y = 1f;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, gridBuildController.GetPlacedObjectRotation(), Time.deltaTime * 15f);

        if (_visual)
        {
            MeshRenderer[] mrs = _visual.GetComponentsInChildren<MeshRenderer>();
            Material selectMat = gridBuildController.CheckCanBuildAtPos() ? _curOriginMat : ghostMaterialDisable;
            foreach (var mr in mrs)
            {
                mr.material = selectMat;
            }
        }
    }

    private void RefreshVisual(BuildObjData placedObjectData) 
    {
        if (_visual != null) 
        {
            Destroy(_visual.gameObject);
            _visual = null;
        }
        
        if (placedObjectData != null) 
        {
            _visual = Instantiate(placedObjectData.prefab, Vector3.zero, Quaternion.identity);
            _visual.parent = transform;
            _visual.localPosition = Vector3.zero;
            _visual.localEulerAngles = Vector3.zero;
            MeshRenderer[] mrs = _visual.GetComponentsInChildren<MeshRenderer>();
            
            _curOriginMat = (gridBuildController.CanBuildObject())
                ? ghostMaterialEnable
                : ghostMaterialDisable;
            foreach (var mr in mrs)
            {
                mr.material = _curOriginMat;
            }
            SetLayerRecursive(_visual.gameObject, 15);
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer) 
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform) 
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

}
