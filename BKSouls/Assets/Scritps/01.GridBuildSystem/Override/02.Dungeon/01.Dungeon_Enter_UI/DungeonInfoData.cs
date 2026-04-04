using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Info/DungeonDataList ")]
public class DungeonInfoData : ScriptableObject
{
    public List<DungeonData> dungeonDataList;
}
