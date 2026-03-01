using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Dungeon/Editor/RoomListData ")]
public class DungeonRoomListDataSO : ScriptableObject
{
    public List<DungeonRoomDataSO> essentialRoom;
    public List<DungeonRoomDataSO> subRoom;
}



