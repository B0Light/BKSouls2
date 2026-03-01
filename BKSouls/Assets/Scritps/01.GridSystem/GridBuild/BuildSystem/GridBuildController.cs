using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class GridBuildController : MonoBehaviour
{
    [SerializeField] private BaseGridBuildSystem baseGridBuildSystem;
    
    private BuildObjData _objectToPlace;
    
    private Dir _dir = Dir.Down;
    
    private bool _isDragging = false;
    private bool _isActive = false;
    private readonly Variable<bool> _isDeleteMode = new Variable<bool>(false);
    private Vector2Int _lastPlacedPosition;
    
    [Space(10)] 
    [SerializeField] private GameObject selector;
    [SerializeField] private MeshRenderer selectorMeshRenderer;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material selectMat;
    [SerializeField] private Material disableMat;
    [SerializeField] private Material deleteMat;
    
    private void OnEnable()
    {
        _lastPlacedPosition = new Vector2Int(-1, -1);
        BaseGridBuildSystem.OnSelectedChanged += SetObjectToPlace;
        _isDeleteMode.OnValueChanged += SetSelectorMat;
    }

    private void OnDisable()
    {
        BaseGridBuildSystem.OnSelectedChanged -= SetObjectToPlace;
        _isDeleteMode.OnValueChanged -= SetSelectorMat;
    }
    
    private void SetSelectorMat(bool newValue)
    {
        selectorMeshRenderer.material = newValue ? deleteMat : baseMat;
    }

    private void SetObjectToPlace(BuildObjData buildObjData)
    {
        _objectToPlace = buildObjData;
        _isActive = true;
    }
    
    private void Update()
    {
        if (!_isActive)
        {
            _objectToPlace = null;
            selector.SetActive(false);
            return;
        }
        
        selector.SetActive(_objectToPlace == null);
        
        if (_objectToPlace == null)
        {
            Vector3 targetPosition = GetMouseWorldSnappedPosition();
            baseGridBuildSystem.GetGrid().GetXZ(targetPosition, out int x, out int z);
            
            GridCell gridObject = baseGridBuildSystem.GetGrid().GetGridObject(x, z);
            PlacedObject placedObject = gridObject?.GetPlacedObject();

            if (placedObject)
            {
                BuildObjData obj = placedObject.GetBuildObjData();
                var dir = placedObject.GetDir();
                selector.transform.localScale = new Vector3(obj.GetWidth(dir),1,obj.GetHeight(dir));

                targetPosition = baseGridBuildSystem.GetGrid().GetWorldPosition(placedObject.GetOriginPos());
                if(_isDeleteMode.Value == false)
                    selectorMeshRenderer.material = obj.GetCellType() == CellType.Empty ? baseMat : selectMat;
            }
            else
            {
                selector.transform.localScale = new Vector3(1, 1, 1);
                if(_isDeleteMode.Value == false)
                    selectorMeshRenderer.material = disableMat;
            }
            targetPosition.y = 0.25f;
            selector.transform.position = Vector3.Lerp(selector.transform.position, targetPosition, Time.deltaTime * 15f);
        }
        

        if (Input.GetMouseButtonDown(1))
        {
            baseGridBuildSystem.SelectToBuild(null);
            _isDeleteMode.Value = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                _isDragging = false;
                return;
            }

            if (_objectToPlace != null)
            {
                _isDragging = true;
            }
            else
            {
                _isDragging = false;
                if (_isDeleteMode.Value)
                {
                    RemoveObjectAtMousePosition();
                }
                else
                {
                    SelectObjectAtMousePosition();
                }
            }
        }

        if (Input.GetMouseButton(0) && _isDragging)
        {
            PlaceObjectAtMousePositionIfNeeded();
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;
            _lastPlacedPosition = new Vector2Int(-1, -1); // 드래그 종료 시 마지막 위치 초기화
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            _dir = BuildObjData.GetNextDir(_dir);
        }

        if (_objectToPlace == null || _objectToPlace?.GetCellType() == CellType.Road)
        {
            _dir = Dir.Down;
        }
    }
    
    private void PlaceObjectAtMousePositionIfNeeded()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        baseGridBuildSystem.GetGrid().GetXZ(mousePosition, out int x, out int z);
        Vector2Int currentGridPosition = new Vector2Int(x, z);

        // 중복 배치 방지
        if (currentGridPosition == _lastPlacedPosition) return;

        if (CheckCanBuildAtPos(x,z))
        {
            if (CheckItemInInventory(_objectToPlace) && SpendItemInInventory(_objectToPlace))
            {
                baseGridBuildSystem.PlaceTile(x, z, _dir);
                _lastPlacedPosition = currentGridPosition; // 마지막 배치 위치 갱신
            }
            else
            {
                Debug.Log("No Item");
            }
        }
        else
        {
            Debug.Log("Can Not Build Here");
        }
    }
    
    public bool CanBuildObject()
    {
        return _objectToPlace && CheckItemInInventory(_objectToPlace);
    }
    
    private bool CheckItemInInventory(BuildObjData buyObject)
    {
        foreach (var costItem in buyObject.costItemDic)
        {
            if (!WorldPlayerInventory.Instance.CheckItemInInventory(costItem.Key, costItem.Value))
                return false;
        }

        return true;
    }

    // CheckItemInInventory 에서 재고를 확인 이후 제거함
    private bool SpendItemInInventory(BuildObjData buyObject)
    {
        foreach (var costItem in buyObject.costItemDic)
        {
            if (!WorldPlayerInventory.Instance.RemoveItemInInventory(costItem.Key, costItem.Value))
                return false;
        }

        return true;
    }
    
    private bool CheckCanBuildAtPos(int x, int z)
    {
        return IsPlacementValid(x, z) && BaseGridBuildSystem.Instance.CanBuildAtPos(_objectToPlace.GetGridPositionList(new Vector2Int(x, z), _dir));
    }

    public bool CheckCanBuildAtPos()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        baseGridBuildSystem.GetGrid().GetXZ(mousePosition, out int x, out int z);
        return IsPlacementValid(x, z) && BaseGridBuildSystem.Instance.CanBuildAtPos(_objectToPlace.GetGridPositionList(new Vector2Int(x, z), _dir));
    }
    
    private bool IsPlacementValid(int x, int z)
    {
        if (_objectToPlace == null)
        {
            Debug.Log("Object is Null");
            return false;
        }
        
        int objectWidth = _objectToPlace.GetWidth(_dir);
        int objectLength = _objectToPlace.GetHeight(_dir);

        return x >= 0 && z >= 0 &&
               x + objectWidth <= baseGridBuildSystem.GetGrid().Width &&
               z + objectLength <= baseGridBuildSystem.GetGrid().Height;
    }
    
    private void RemoveObjectAtMousePosition()
    {
        // 손에 배치할 타일이 있으면 타일 제거 불가 
        if(_objectToPlace) return;
        
        PlacedObject placedObject = GetObjectAtMousePosition();
        
        baseGridBuildSystem.RemoveTile(placedObject);
    }

    private void SelectObjectAtMousePosition()
    {
        if (_objectToPlace) return;
        PlacedObject placedObject = GetObjectAtMousePosition();
    
        if (placedObject is RevenueFacilityTile attractionTile)
        {
            GridBuildHUDManager.Instance.OpenBuildPopUpHUD(attractionTile);
        }
    }
    
    public Vector3 GetMouseWorldSnappedPosition() {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        baseGridBuildSystem.GetGrid().GetXZ(mousePosition, out int x, out int z);

        Vector3 placedObjectWorldPosition = baseGridBuildSystem.GetGrid().GetWorldPosition(x, z);
        if (_objectToPlace != null) {
            Vector2Int rotationOffset = _objectToPlace.GetRotationOffset(_dir);
            placedObjectWorldPosition += new Vector3(rotationOffset.x, 0, rotationOffset.y) * baseGridBuildSystem.GetGrid().CellSize;
        } 
        
        return placedObjectWorldPosition;
    }
    
    private PlacedObject GetObjectAtMousePosition()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        baseGridBuildSystem.GetGrid().GetXZ(mousePosition, out int x, out int z);

        GridCell gridObject = baseGridBuildSystem.GetGrid().GetGridObject(x, z);
        return gridObject?.GetPlacedObject();
    }
    
    public Quaternion GetPlacedObjectRotation() {
        return _objectToPlace ? Quaternion.Euler(0, _objectToPlace.GetRotationAngle(_dir), 0) : Quaternion.identity;
    }
    
    public void SetDeleteMode()
    {
        baseGridBuildSystem.SelectToBuild(null);

        _isDeleteMode.Value = !_isDeleteMode.Value;
    }
    
    public void SetControllerActive(bool value) => _isActive = value;
}
