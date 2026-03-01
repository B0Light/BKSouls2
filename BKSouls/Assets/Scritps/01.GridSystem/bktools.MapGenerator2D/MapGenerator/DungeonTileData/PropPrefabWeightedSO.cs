using UnityEngine;

[CreateAssetMenu(fileName = "New Prop Prefab", menuName = "Dungeon/Editor/Prop Prefab")]
public class PropPrefabWeightedSO : ScriptableObject
{
    public GameObject prefab;
    [Range(0,10)]public int weight = 1;
}