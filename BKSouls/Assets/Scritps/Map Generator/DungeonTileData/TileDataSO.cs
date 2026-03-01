using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Dungeon/TileData")]
public class TileDataSO : ScriptableObject
{
    public GameObject tilePrefab;
    
    [Header("Prop Settings")]
    [Range(0, 100)] public int objectPercent;
    [Range(0, 100)] public int treePercent;
    [Range(0, 100)] public int grassPercent;

    [Header("Weighted Prop Prefabs")]
    public List<PropPrefabWeightedSO> objectPrefabs;
    public List<PropPrefabWeightedSO> treePrefabs;
    public List<PropPrefabWeightedSO> grassPrefabs;

    public GameObject SpawnTile(Vector3 position, Vector3 size, Transform parent, bool isSpawnPoint = false)
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is missing.");
            return null;
        }

        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
        tile.transform.localScale = size;
        tile.transform.SetParent(parent);
        if(!isSpawnPoint) SpawnProps(tile.transform);
        return tile;
    }
    
    public GameObject SpawnTileWithSizeAwareProps(Vector3 position, Vector3 size, Transform parent, RoomInfo roomInfo)
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is missing.");
            return null;
        }

        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
        tile.transform.localScale = size;
        tile.transform.SetParent(parent);
        
        // 방 크기를 고려한 프롭 생성
        SpawnSizeAwareProps(tile.transform, roomInfo);
        
        return tile;
    }

    private void SpawnProps(Transform parent)
    {
        if (TrySpawnProp(objectPrefabs, objectPercent, parent, GetRandomYRotation90()))
            return;

        if (TrySpawnProp(treePrefabs, treePercent, parent, GetRandomYRotation()))
            return;

        TrySpawnProp(grassPrefabs, grassPercent, parent, GetRandomYRotation());
    }

    private bool TrySpawnProp(List<PropPrefabWeightedSO> prefabList, int percentChance, Transform parent, Quaternion rotation)
    {
        if (prefabList == null || prefabList.Count == 0) return false;
        if (!IsChanceSuccessful(percentChance)) return false;

        PropPrefabWeightedSO propData = GetWeightedRandomPrefab(prefabList);
        if (propData == null || propData.prefab == null) return false;

        Transform propsTransform = parent.transform.Find("Props");

        if (propsTransform == null) return false;
        
        GameObject childInstance = Instantiate(propData.prefab, propsTransform);
        childInstance.transform.localPosition = Vector3.zero;
        childInstance.transform.rotation = rotation;
        
        return true;
    }
    
    // 방 크기를 고려하여 프롭 생성
    private void SpawnSizeAwareProps(Transform parent, RoomInfo roomInfo)
    {
        // Props 자식 오브젝트 확인
        Transform propsTransform = parent.transform.Find("Props");
        if (propsTransform == null) return;
        
        // 방의 크기를 확인하여 적합한 프롭 선택 (가장 작은 차원 사용)
        Vector2Int roomSize = roomInfo.size;
        int minRoomDimension = Mathf.Min(roomSize.x, roomSize.y);
        
        // 1. 방 크기에 맞는 Object 프롭 시도
        TrySpawnSizeAwareProp(objectPrefabs, objectPercent, propsTransform, minRoomDimension);
        
        // 2. 방 크기에 맞는 Tree 프롭 시도
        TrySpawnSizeAwareProp(treePrefabs, treePercent, propsTransform, minRoomDimension);
        
        // 3. 방 크기에 맞는 Grass 프롭 시도 (Grass는 일반적으로 작으므로 항상 가능)
        TrySpawnSizeAwareProp(grassPrefabs, grassPercent, propsTransform, minRoomDimension);
    }
    
    // 방 크기를 고려한 프롭 생성 시도
    private bool TrySpawnSizeAwareProp(List<PropPrefabWeightedSO> prefabList, int percentChance, Transform parent, int maxAllowedSize)
    {
        if (prefabList == null || prefabList.Count == 0) return false;
        if (!IsChanceSuccessful(percentChance)) return false;
        
        // 방 크기에 맞는 프롭만 필터링
        List<PropPrefabWeightedSO> compatibleProps = new List<PropPrefabWeightedSO>();
        
        foreach (var prop in prefabList)
        {
            // 프롭 크기가 방에 맞는지 확인 (프롭의 가장 큰 차원이 방의 최소 차원보다 작아야 함)
            int maxPropDimension = Mathf.Max(prop.size.x, prop.size.y);
            
            if (maxPropDimension <= maxAllowedSize)
            {
                compatibleProps.Add(prop);
            }
        }
        
        if (compatibleProps.Count == 0) return false;
        
        // 크기에 맞는 프롭에서 가중치 랜덤 선택
        PropPrefabWeightedSO selectedProp = GetWeightedRandomPrefab(compatibleProps);
        if (selectedProp == null || selectedProp.prefab == null) return false;
        
        // 프롭 생성
        GameObject childInstance = Instantiate(selectedProp.prefab, parent);
        childInstance.transform.localPosition = Vector3.zero;
        childInstance.transform.rotation = GetPropRotation(selectedProp);
        
        return true;
    }
    
    // 프롭 유형에 따른 적절한 회전 선택
    private Quaternion GetPropRotation(PropPrefabWeightedSO prop)
    {
        return treePrefabs.Contains(prop) ? GetRandomYRotation() : GetRandomYRotation90();
    }

    private bool IsChanceSuccessful(int percent)
    {
        return Random.Range(0, 100) < percent;
    }

    private Quaternion GetRandomYRotation90()
    {
        int[] angles = { 0, 90, 180, 270 };
        return Quaternion.Euler(0f, angles[Random.Range(0, angles.Length)], 0f);
    }

    private Quaternion GetRandomYRotation()
    {
        return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    private PropPrefabWeightedSO GetWeightedRandomPrefab(List<PropPrefabWeightedSO> prefabList)
    {
        float totalWeight = 0f;
        foreach (var entry in prefabList)
            totalWeight += entry.weight;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in prefabList)
        {
            cumulative += entry.weight;
            if (randomValue < cumulative)
                return entry;
        }

        return prefabList.Count > 0 ? prefabList[0] : null;
    }
}