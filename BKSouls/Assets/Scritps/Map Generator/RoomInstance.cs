using Unity.Netcode;
using UnityEngine;

namespace BK
{

    public class RoomInstance : NetworkBehaviour
    {
        public Vector2Int RoomPos { get; private set; }
        public RoomType RoomType { get; private set; }
        public int StageSeed { get; private set; }

        public void SetRoomId(Vector2Int pos, RoomType type, int seed)
        {
            RoomPos = pos;
            RoomType = type;
            StageSeed = seed;
        }
    }
}