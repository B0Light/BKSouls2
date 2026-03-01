using BK.Inventory;
using UnityEngine;

public class RoadTile : PlacedObject
{
    [SerializeField] private GameObject defaultPrefab;
    [Space(10)]
    [SerializeField] private GameObject straightPrefab; // 1자 연결 프리팹 (-)
    [SerializeField] private GameObject cornerPrefab;   // ㄱ자 연결 프리팹
    [SerializeField] private GameObject crossPrefab;    // +자 연결 프리팹
    [SerializeField] private GameObject tPrefab;        // ㅗ자 연결 프리팹
    
    public virtual void UpdateConnections()
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // 상
            new Vector2Int(-1, 0), // 좌
            new Vector2Int(0, -1), // 하
            new Vector2Int(1, 0)   // 우
        };

        // 연결 정보를 나타내는 4비트 문자열 생성
        string connectionKey = "";
        
        foreach (var dir in directions)
        {
            Vector2Int checkPos = originPos + dir;
            if (IsRoadAtPosition(checkPos, dir))
            {
                connectionKey += "1";
            }
            else
            {
                connectionKey += "0";
            }
        }

        // 연결 상태에 따라 모델을 업데이트
        UpdateModel(connectionKey);
    }

    protected bool IsRoadAtPosition(Vector2Int position, Vector2Int direction)
    {
        GridCell gridObject = BaseGridBuildSystem.Instance.GetGrid().GetGridObject(position.x, position.y);
        if (gridObject == null)
        {
            return false;
        }

        CellType? tileType = gridObject.CellType;

        switch (tileType)
        {
            case CellType.Road:
                return true;
            case CellType.HQ:
            case CellType.MajorFacility:
                return IsSpecialTileConnectedToRoad(gridObject, position, direction);
            default:
                return false;
        }
    }

    private bool IsSpecialTileConnectedToRoad(GridCell gridObject, Vector2Int position, Vector2Int direction)
    {
        // 입구 확인
        if (position == gridObject.GetEntrancePosition())
        {
            Dir objectDirection = gridObject.GetDirection();
            return objectDirection == ConvertToConnectDirection(direction);
        }
    
        // 출구 확인
        if (position == gridObject.GetExitPosition())
        {
            Dir objectDirection = gridObject.GetExitDirection();
            return objectDirection == ConvertToConnectDirection(direction);
        }
    
        return false;
    }
    
    private static Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return Dir.Right;
        if (direction == new Vector2Int(0, -1)) return Dir.Up;
        if (direction == new Vector2Int(1, 0)) return Dir.Left;
        return Dir.Up;
    }

    protected void UpdateModel(string connectionKey)
    {
        // 기존 모델 제거
        foreach (Transform child in modelSlot)
        {
            Destroy(child.gameObject);
        }

        // 연결 상태에 따른 모델과 회전 결정
        GameObject prefabToInstantiate = null;
        Quaternion rotation = Quaternion.identity;

        switch (connectionKey)
        {
            case "0000":
                prefabToInstantiate = defaultPrefab;
                break;
            case "0001":
            case "0010":
            case "0100":
            case "1000":
                prefabToInstantiate = straightPrefab;
                rotation = (connectionKey == "0010" || connectionKey == "1000") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                break;
            case "0101": // 좌-우 연결 (1자)
            case "1010": // 상-하 연결 (1자)
                prefabToInstantiate = straightPrefab;
                rotation = (connectionKey == "1010") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                break;

            case "1001": // 상-우 연결 (ㄱ자)
            case "1100": // 상-좌 연결 (ㄱ자)
            case "0011": // 좌-하 연결 (ㄱ자)
            case "0110": // 하-우 연결 (ㄱ자)
                prefabToInstantiate = cornerPrefab;
                rotation = GetCornerRotation(connectionKey);
                break;

            case "1110": // 상-좌-우 연결 (ㅗ자)
            case "1011": // 상-하-좌 연결 (ㅗ자)
            case "0111": // 좌-하-우 연결 (ㅗ자)
            case "1101": // 상-하-우 연결 (ㅗ자)
                prefabToInstantiate = tPrefab;
                rotation = GetTRotation(connectionKey);
                break;

            case "1111": // 상-하-좌-우 연결 (+자)
                prefabToInstantiate = crossPrefab;
                break;

            default:
                prefabToInstantiate = defaultPrefab;
                break; // 연결 없음
        }

        // 새 모델 생성 및 적용
        if (prefabToInstantiate != null)
        {
            GameObject iTile = Instantiate(prefabToInstantiate, modelSlot);
            iTile.transform.localPosition = Vector3.zero;
            iTile.transform.localRotation = rotation;
        }
    }

    private Quaternion GetCornerRotation(string connectionKey)
    {
        switch (connectionKey)
        {
            case "0110": return Quaternion.Euler(0, 0, 0);   
            case "1100": return Quaternion.Euler(0, 90, 0); 
            case "1001": return Quaternion.Euler(0, 180, 0); 
            case "0011": return Quaternion.Euler(0, 270, 0);  
            default: return Quaternion.identity;
        }
    }

    private Quaternion GetTRotation(string connectionKey)
    {
        switch (connectionKey)
        {
            case "0111": return Quaternion.Euler(0, 0, 0); 
            case "1110": return Quaternion.Euler(0, 90, 0);   
            case "1101": return Quaternion.Euler(0, 180, 0);  
            case "1011": return Quaternion.Euler(0, 270, 0); 
            default: return Quaternion.identity;
        }
    }

}
