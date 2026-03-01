using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dungeon/Editor/TileData")]
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

    public GameObject SpawnTile(Vector3 position, Vector3 size, Transform parent)
    {
        GameObject tile = SpawnBasicTile(position, size, parent);
        
        SpawnProps(tile.transform);
        
        return tile;
    }

    private GameObject SpawnBasicTile(Vector3 position, Vector3 size, Transform parent)
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("Tile prefab is missing.");
            return null;
        }

        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
        tile.transform.localScale = size;
        tile.transform.SetParent(parent);
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