using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomInfo
{
    public Vector2Int position;  // 그리드 상의 좌측 하단 위치
    public Vector2Int size;      // 방 크기
    public Vector2Int center;    // 그리드 상의 중심점
    public Vector3 worldPosition; // 월드 좌표계에서의 좌측 하단 위치
    public Vector3 worldCenter;   // 월드 좌표계에서의 중심점

    public override string ToString()
    {
        return $"Room at {position}, size: {size}, center: {center}";
    }
}

public class RoomNode
{
    public RectInt NodeRect;
    public RectInt RoomRect;
    public RoomNode Left;
    public RoomNode Right;

    public RoomNode(RectInt rect) => NodeRect = rect;

    public Vector2Int GetRoomCenter()
    {
        if (RoomRect.width > 0 && RoomRect.height > 0)
            return new Vector2Int(RoomRect.x + RoomRect.width / 2, RoomRect.y + RoomRect.height / 2);

        Vector2Int center = new Vector2Int(NodeRect.x + NodeRect.width / 2, NodeRect.y + NodeRect.height / 2);
        if (Left != null) return Left.GetRoomCenter();
        if (Right != null) return Right.GetRoomCenter();
        return center;
    }
    
    /// <summary>
    /// 실제 방이 있는 노드의 중심점을 반환합니다. 방이 없으면 null을 반환합니다.
    /// </summary>
    public Vector2Int? GetActualRoomCenter()
    {
        if (RoomRect.width > 0 && RoomRect.height > 0)
            return new Vector2Int(RoomRect.x + RoomRect.width / 2, RoomRect.y + RoomRect.height / 2);
            
        return null;
    }
    
    /// <summary>
    /// 하위 노드들 중에서 실제 방이 있는 모든 노드의 중심점을 수집합니다.
    /// </summary>
    public List<Vector2Int> GetAllRoomCenters()
    {
        List<Vector2Int> centers = new List<Vector2Int>();
        CollectRoomCenters(centers);
        return centers;
    }
    
    private void CollectRoomCenters(List<Vector2Int> centers)
    {
        // 현재 노드에 방이 있으면 추가
        if (RoomRect.width > 0 && RoomRect.height > 0)
        {
            centers.Add(new Vector2Int(RoomRect.x + RoomRect.width / 2, RoomRect.y + RoomRect.height / 2));
        }
        
        // 자식 노드들도 확인
        if (Left != null) Left.CollectRoomCenters(centers);
        if (Right != null) Right.CollectRoomCenters(centers);
    }
    
    /// <summary>
    /// 가장 가까운 실제 방의 중심점을 반환합니다.
    /// </summary>
    public Vector2Int? FindNearestRoomCenter(Vector2Int targetPos)
    {
        var allCenters = GetAllRoomCenters();
        if (allCenters.Count == 0) return null;
        
        Vector2Int? nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (var center in allCenters)
        {
            float distance = Vector2Int.Distance(center, targetPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = center;
            }
        }
        
        return nearest;
    }
}

public class Room
{
    public Vector2Int Position;
    public int Width;
    public int Height;
    public RoomType Type;
    public List<Vector2Int> Doors = new List<Vector2Int>();

    public Room(Vector2Int pos, int width, int height, RoomType type)
    {
        Position = pos;
        Width = width;
        Height = height;
        Type = type;
    }
}

[Serializable]
public struct WaypointData
{
    public Vector3[] waypoints;
    
    public WaypointData(Vector3[] waypoints)
    {
        this.waypoints = waypoints;
    }
}

public enum RoomType
{
    Start,
    Normal,
    Special
}


