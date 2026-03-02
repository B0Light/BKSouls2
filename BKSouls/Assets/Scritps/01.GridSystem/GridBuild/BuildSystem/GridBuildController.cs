using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseGridBuildSystem baseGridBuildSystem;

    [Header("Selector")]
    [SerializeField] private GameObject selector;
    [SerializeField] private MeshRenderer selectorMeshRenderer;
    [SerializeField] private Material baseMat;
    [SerializeField] private Material selectMat;
    [SerializeField] private Material disableMat;
    [SerializeField] private Material deleteMat;

    private BuildObjData _objectToPlace;
    private Dir _dir = Dir.Down;

    private bool _isDragging;
    private bool _isActive;

    private readonly Variable<bool> _isDeleteMode = new Variable<bool>(false);
    private Vector2Int _lastPlacedPosition = new Vector2Int(-1, -1);

    private PlayerControls _controls;

    // input flags (edge-trigger)
    private bool _leftClick;
    private bool _leftClickEnd;
    private bool _rightClick;
    private bool _rotateClick;

    private void OnEnable()
    {
        _lastPlacedPosition = new Vector2Int(-1, -1);

        BaseGridBuildSystem.OnSelectedChanged += SetObjectToPlace;
        _isDeleteMode.OnValueChanged += ApplySelectorMaterialForMode;

        EnsureControls();
        _controls.Enable();
    }

    private void OnDisable()
    {
        BaseGridBuildSystem.OnSelectedChanged -= SetObjectToPlace;
        _isDeleteMode.OnValueChanged -= ApplySelectorMaterialForMode;

        if (_controls != null) _controls.Disable();
    }

    private void EnsureControls()
    {
        if (_controls != null) return;

        _controls = new PlayerControls();

        _controls.UI.Click.performed += _ => _leftClick = true;
        _controls.UI.Click.canceled  += _ => _leftClickEnd = true;
        _controls.UI.RightClick.performed += _ => _rightClick = true;
        _controls.UI.Rotate.performed += _ => _rotateClick = true;
    }

    private void Update()
    {
        if (!TryGetGrid(out var grid))
        {
            // baseGridBuildSystem / grid 미할당이면 안전 종료
            selector.SetActive(false);
            ConsumeInputFlags();
            return;
        }

        if (!_isActive)
        {
            // 컨트롤러 비활성 상태면 모든 UI/선택 초기화
            _objectToPlace = null;
            selector.SetActive(false);
            ConsumeInputFlags();
            return;
        }

        // selector는 "손에 들고있는 오브젝트가 없을 때"만 보여주는 용도(기존 코드 유지)
        selector.SetActive(_objectToPlace == null);

        UpdateRotationRule();
        UpdateSelectorVisual(grid);

        HandleRightClick();
        HandleLeftClick(grid);
        HandleLeftClickEnd();
        HandleRotateClick();

        // 프레임 말에 입력 플래그 소모(이 패턴 쓰면 실수 줄어듦)
        ConsumeInputFlags();
    }

    private bool TryGetGrid(out FixedGridXZ<GridCell> grid)
    {
        grid = baseGridBuildSystem != null ? baseGridBuildSystem.GetGrid() : null;
        return grid != null;
    }

    private void SetObjectToPlace(BuildObjData buildObjData)
    {
        _objectToPlace = buildObjData;
        _isActive = true;
    }

    private void ApplySelectorMaterialForMode(bool isDeleteMode)
    {
        selectorMeshRenderer.material = isDeleteMode ? deleteMat : baseMat;
    }

    private void UpdateRotationRule()
    {
        // 기존 로직 유지: Road 또는 손에 아무것도 없으면 Dir.Down 고정
        if (_objectToPlace == null || _objectToPlace.GetCellType() == CellType.Road)
            _dir = Dir.Down;
    }

    private void UpdateSelectorVisual(FixedGridXZ<GridCell> grid)
    {
        if (_objectToPlace != null) return; // 손에 들고있으면 selector 표시/갱신 필요 없음(기존과 동일)

        if (!TryGetMouseGridPos(grid, out int x, out int z))
        {
            selectorMeshRenderer.material = disableMat;
            return;
        }

        var placedObject = grid.GetGridObject(x, z)?.GetPlacedObject();

        Vector3 targetPosition;
        if (placedObject)
        {
            var obj = placedObject.GetBuildObjData();
            var dir = placedObject.GetDir();

            selector.transform.localScale = new Vector3(obj.GetWidth(dir), 1, obj.GetHeight(dir));
            targetPosition = grid.GetWorldPosition(placedObject.GetOriginPos());

            if (!_isDeleteMode.Value)
                selectorMeshRenderer.material = (obj.GetCellType() == CellType.Empty) ? baseMat : selectMat;
        }
        else
        {
            selector.transform.localScale = Vector3.one;
            targetPosition = grid.GetWorldPosition(x, z);

            if (!_isDeleteMode.Value)
                selectorMeshRenderer.material = disableMat;
        }

        targetPosition.y = 0.25f;
        selector.transform.position = Vector3.Lerp(selector.transform.position, targetPosition, Time.deltaTime * 15f);
    }

    private void HandleRightClick()
    {
        if (!_rightClick) return;

        baseGridBuildSystem.SelectToBuild(null);
        _isDeleteMode.Value = false;
    }

    private void HandleLeftClick(FixedGridXZ<GridCell> grid)
    {
        if (!_leftClick) return;

        // UI 위 클릭은 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            _isDragging = false;
            return;
        }

        // 손에 들고있는 오브젝트가 있으면 배치(드래그 가능)
        if (_objectToPlace != null)
        {
            _isDragging = true;
            TryPlaceObjectAtMouse(grid);
            return;
        }

        // 손에 아무것도 없으면 삭제 모드/선택 모드
        _isDragging = false;

        if (_isDeleteMode.Value) TryRemoveObjectAtMouse(grid);
        else TrySelectObjectAtMouse(grid);
    }

    private void HandleLeftClickEnd()
    {
        if (!_leftClickEnd || !_isDragging) return;

        _isDragging = false;
        _lastPlacedPosition = new Vector2Int(-1, -1);
    }

    private void HandleRotateClick()
    {
        if (!_rotateClick) return;

        _dir = BuildObjData.GetNextDir(_dir);
    }

    private void ConsumeInputFlags()
    {
        _leftClick = false;
        _leftClickEnd = false;
        _rightClick = false;
        _rotateClick = false;
    }

    private bool TryGetMouseGridPos(FixedGridXZ<GridCell> grid, out int x, out int z)
    {
        Vector3 mouseWorld = Mouse3D.GetMouseWorldPosition();
        grid.GetXZ(mouseWorld, out x, out z);
        return x >= 0 && z >= 0 && x < grid.Width && z < grid.Height;
    }

    private void TryPlaceObjectAtMouse(FixedGridXZ<GridCell> grid)
    {
        if (_objectToPlace == null) return;

        if (!TryGetMouseGridPos(grid, out int x, out int z))
            return;

        Vector2Int current = new Vector2Int(x, z);

        // 드래그 중 같은 셀 중복 배치 방지
        if (current == _lastPlacedPosition) return;

        if (!IsPlacementValid(grid, x, z))
        {
            Debug.Log("Can Not Build Here");
            return;
        }

        if (!CanBuildAtPos(x, z))
        {
            Debug.Log("Can Not Build Here");
            return;
        }

        if (!CheckItemInInventory(_objectToPlace) || !SpendItemInInventory(_objectToPlace))
        {
            Debug.Log("No Item");
            return;
        }

        baseGridBuildSystem.PlaceTile(x, z, _dir);
        _lastPlacedPosition = current;
    }

    public bool CanBuildObject()
    {
        return _objectToPlace != null && CheckItemInInventory(_objectToPlace);
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

    private bool SpendItemInInventory(BuildObjData buyObject)
    {
        foreach (var costItem in buyObject.costItemDic)
        {
            if (!WorldPlayerInventory.Instance.RemoveItemInInventory(costItem.Key, costItem.Value))
                return false;
        }
        return true;
    }

    private bool CanBuildAtPos(int x, int z)
    {
        return BaseGridBuildSystem.Instance.CanBuildAtPos(
            _objectToPlace.GetGridPositionList(new Vector2Int(x, z), _dir)
        );
    }

    public bool CheckCanBuildAtPos()
    {
        if (!TryGetGrid(out var grid)) return false;
        if (_objectToPlace == null) return false;
        if (!TryGetMouseGridPos(grid, out int x, out int z)) return false;

        return IsPlacementValid(grid, x, z) && CanBuildAtPos(x, z);
    }

    private bool IsPlacementValid(FixedGridXZ<GridCell> grid, int x, int z)
    {
        if (_objectToPlace == null) return false;

        int w = _objectToPlace.GetWidth(_dir);
        int h = _objectToPlace.GetHeight(_dir);

        return x >= 0 && z >= 0 &&
               x + w <= grid.Width &&
               z + h <= grid.Height;
    }

    private void TryRemoveObjectAtMouse(FixedGridXZ<GridCell> grid)
    {
        if (_objectToPlace != null) return; // 손에 들고있으면 제거 불가(기존 정책)

        var placed = GetObjectAtMouse(grid);
        if (!placed) return;               // NRE 방지(기존 코드는 null일 때 RemoveTile로 갈 수 있었음)

        baseGridBuildSystem.RemoveTile(placed);
    }

    private void TrySelectObjectAtMouse(FixedGridXZ<GridCell>grid)
    {
        if (_objectToPlace != null) return;

        var placed = GetObjectAtMouse(grid);
        if (!placed) return;

        if (placed is RevenueFacilityTile attractionTile)
            GridBuildHUDManager.Instance.OpenBuildPopUpHUD(attractionTile);
    }

    public Vector3 GetMouseWorldSnappedPosition()
    {
        if (!TryGetGrid(out var grid)) return Vector3.zero;

        Vector3 mouseWorld = Mouse3D.GetMouseWorldPosition();
        grid.GetXZ(mouseWorld, out int x, out int z);

        Vector3 pos = grid.GetWorldPosition(x, z);

        if (_objectToPlace != null)
        {
            Vector2Int offset = _objectToPlace.GetRotationOffset(_dir);
            pos += new Vector3(offset.x, 0, offset.y) * grid.CellSize;
        }

        return pos;
    }

    private PlacedObject GetObjectAtMouse(FixedGridXZ<GridCell>grid)
    {
        if (!TryGetMouseGridPos(grid, out int x, out int z))
            return null;

        return grid.GetGridObject(x, z)?.GetPlacedObject();
    }

    public Quaternion GetPlacedObjectRotation()
    {
        return _objectToPlace != null
            ? Quaternion.Euler(0, _objectToPlace.GetRotationAngle(_dir), 0)
            : Quaternion.identity;
    }

    public void SetDeleteMode()
    {
        baseGridBuildSystem.SelectToBuild(null);
        _isDeleteMode.Value = !_isDeleteMode.Value;
    }

    public void SetControllerActive(bool value) => _isActive = value;
}