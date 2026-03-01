using System.Collections.Generic;
using BK;
using BK.Inventory;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    protected BuildObjData buildObjData;
    
    protected Transform modelSlot;
    private Transform _propsSlot;
    private Transform _entrancePoint;
    protected Vector2Int originPos; // 마우스 위치에 해당하는 위치의 그리드 좌표 
    private Dir _dir;
    protected int level;
    private int _fee;
    
    /* 입구 및 출구 - shop 및 건설시 정보 를 보이기 위해 Public 으로 설정 */
    public Dir exitDir;
    public int entrancePos;
    public int exitPos;
    
    private GameObject _vCam;
    
    public bool Irremovable { get; private set; }

    protected virtual void Awake()
    {
        modelSlot = FindChildByName(gameObject, "Mesh").transform;
        _propsSlot = FindChildByName(gameObject, "Props").transform;
        _entrancePoint = FindChildByName(gameObject, "Entrance").transform;
        
        _vCam = FindChildByName(gameObject, "CinemachineCamera");
    }
    
    protected GameObject FindChildByName(GameObject parent, string targetName)
    {
        // 모든 자식 오브젝트를 탐색
        foreach (Transform child in parent.transform)
        {
            if (child.name == targetName)
            {
                return child.gameObject; // 이름이 일치하는 오브젝트를 반환
            }
        }
        
        return modelSlot ? modelSlot.gameObject : this.gameObject;
    }

    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, Dir dir,
        BuildObjData buildObjData,
        int level,
        bool isDefault = false)
    {
        // 일단 타일 건설 
        Transform placedObjectTransform = Instantiate(buildObjData.prefab, worldPosition,
            Quaternion.Euler(0, buildObjData.GetRotationAngle(dir), 0));
        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
        
        // 데이터 갱신 
        placedObject.buildObjData = buildObjData;
        placedObject.originPos = origin;
        placedObject._dir = dir;
        placedObject.level = level;
        placedObject.Irremovable = isDefault;
        placedObject._fee = buildObjData.baseFee;
        
        // 해당 클래스에서 건물 건설
        placedObject.BuildProp(level);
        placedObject.SetToSpecificLevel(level);
        
        // RoadTile로 캐스팅
        RoadTile roadTile = placedObject.GetComponent<RoadTile>();
        if (roadTile != null)
        {
            // RoadTile의 초기화 작업 수행
            roadTile.UpdateConnections();
        }
        
        return placedObject;
    }

    private void BuildProp(int buildLevel)
    {
        int index = Mathf.Min(buildLevel, buildObjData.upgradeStages.Count - 1);
        if (buildObjData.upgradeStages.Count == 0 || buildObjData.upgradeStages[index].prop == null) return;

        ClearChildren(_propsSlot);

        // 범위를 초과하면 마지막 단계의 프롭을 사용
        
        Instantiate(buildObjData.upgradeStages[index].prop, _propsSlot);
    }

    
    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    public virtual void UpgradeTile()
    {
        level++;
        _fee = GetFee();
        
        BuildProp(level);
        SetToSpecificLevel(level);
    }

    private void SetToSpecificLevel(int setLevel)
    {
        Interactable interactable = GetComponentInChildren<Interactable>();
        if (interactable)
        {
            interactable.SetToSpecificLevel(setLevel);
        }
    }
    
    public int GetFee() {
        if (level <= 3)
            return _fee + (level - 1) * 50;
        else
            return Mathf.RoundToInt(_fee * Mathf.Pow(1.2f, level - 3));
    }
    
    public int GetFeeByLevel(int level) {
        if (level <= 3)
            return _fee + (level - 1) * 50;
        else
            return Mathf.RoundToInt(_fee * Mathf.Pow(1.3f, level - 3));
    }
    
    public int GetUpgradeCost()
    {
        int nextLevel = level + 1;

        // 최대 레벨 도달 시
        if (nextLevel > buildObjData.maxLevel)
        {
            Debug.LogWarning("최대 레벨을 초과했습니다.");
            return -1; // 또는 throw new InvalidOperationException("...");
        }

        // 프리셋된 업그레이드 단계가 존재하면 그것을 사용
        if (nextLevel < buildObjData.upgradeStages.Count)
        {
            return buildObjData.upgradeStages[nextLevel].cost;
        }

        // 프리셋 외의 단계는 수학적으로 계산
        return CalculateUpgradeCost(nextLevel);
    }
    
    private int CalculateUpgradeCost(int level)
    {
        return Mathf.RoundToInt(buildObjData.cost * Mathf.Pow(1.2f, level - buildObjData.upgradeStages.Count + 1));
    }

    public void SelectObject(bool value)
    {
        _vCam.SetActive(value);
    }

    public int GetTotalUpgradeCost()
    {
        int totalCost = 0;

        // 0 번은 레벨 인자를 맞추기 위해 기본 값으로 추가해둠 
        for (int i = 1; i <= level; i++)
        {
            if (i < buildObjData.upgradeStages.Count)
            {
                totalCost += buildObjData.upgradeStages[i].cost;
            }
            else
            {
                totalCost += Mathf.RoundToInt(buildObjData.cost * Mathf.Pow(1.2f, i - buildObjData.upgradeStages.Count + 1));
            }
        }

        return totalCost;
    }

    
    public List<Vector2Int> GetGridPositionList()
    {
        return buildObjData.GetGridPositionList(originPos, _dir);
    }
    
    public Vector2Int GetEntrance()
    {
        Vector2Int entrance = originPos;
        switch (_dir)
        {
            case Dir.Down:
                entrance.x += entrancePos;
                break;
            case Dir.Left:
                entrance.y += buildObjData.GetHeight(_dir) - entrancePos - 1;
                break;
            case Dir.Up:
                entrance.x += buildObjData.GetWidth(_dir) - entrancePos  - 1;
                entrance.y += buildObjData.GetHeight(_dir) - 1;
                break;
            case Dir.Right:
                entrance.x += buildObjData.GetWidth(_dir) - 1;
                entrance.y += entrancePos;
                break;
            default:
                break;
        }
        return entrance;
    }

    public Vector2Int GetExit()
    {
        Vector2Int exit = originPos;
    
        switch (GetActualExitDirection())
        {
            case Dir.Down:
                exit.x += exitPos;
                break;
            case Dir.Left:
                exit.y += buildObjData.GetHeight(_dir) - exitPos - 1;
                break;
            case Dir.Up:
                exit.x += buildObjData.GetWidth(_dir) - exitPos - 1;
                exit.y += buildObjData.GetHeight(_dir) - 1;
                break;
            case Dir.Right:
                exit.x += buildObjData.GetWidth(_dir) - 1;
                exit.y += exitPos;
                break;
            default:
                break;
        }
        return exit;
    }

    protected Vector2Int GetExitRoad()
    {
        Vector2Int exit = GetExit();
    
        switch (GetActualExitDirection())
        {
            case Dir.Down:
                exit.y -= 1; // 아래쪽으로 한 칸 이동
                break;
            case Dir.Left:
                exit.x -= 1; // 왼쪽으로 한 칸 이동
                break;
            case Dir.Up:
                exit.y += 1; // 위쪽으로 한 칸 이동
                break;
            case Dir.Right:
                exit.x += 1; // 오른쪽으로 한 칸 이동
                break;
            default:
                break;
        }
        return exit;
    }

    public Dir GetActualExitDirection()
    {
        // Down을 0, Left를 1, Up을 2, Right를 3으로 간주하고 회전 계산
        int baseIndex = (int)_dir;
        int rotationIndex = (int)exitDir;
    
        // (baseIndex + rotationIndex) % 4로 실제 방향 계산
        int actualIndex = (baseIndex + rotationIndex) % 4;
    
        return (Dir)actualIndex;
    }

    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }
    
    public BuildObjData GetBuildObjData()
    {
        return buildObjData;
    }
    
    public Dir GetDir()
    {
        return _dir;
    }

    public bool IsDefault()
    {
        return buildObjData.itemID == 0 || buildObjData.itemID == 10;
    }

    public Vector3 GetEntrancePoint()
    {
        return _entrancePoint.transform.position;
    }
    
    public Vector2Int GetOriginPos()
    {
        return originPos;
    }

    public int GetLevel()
    {
        return level;
    }
}
