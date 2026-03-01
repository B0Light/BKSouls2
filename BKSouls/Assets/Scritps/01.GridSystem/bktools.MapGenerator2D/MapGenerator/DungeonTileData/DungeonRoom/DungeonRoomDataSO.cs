using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dungeon/Editor/RoomData ")]
public class DungeonRoomDataSO : ScriptableObject
{
    public List<DungeonObjectDataSO> props;
}
