using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{

    [CreateAssetMenu(menuName = "Build/ObjectData")]
    public class BuildObjData : GridItem
    {
        [System.Serializable]
        public struct UpgradeStage
        {
            public Transform prop;
            public int cost;
        }

        public Transform prefab;
        
        public Dictionary<int,int> costItemDic = new Dictionary<int,int>();

        [Space(10)] 
        [SerializeField] private CellType cellType; // 건설할 타일의 타입

        public int maxLevel = 10;
        public int baseFee = 100;

        public List<UpgradeStage> upgradeStages = new List<UpgradeStage>();

        // Direction-related utilities
        public static Dir GetNextDir(Dir dir) => (Dir)(((int)dir + 1) % 4);

        public int GetRotationAngle(Dir dir) => ((int)dir * 90) % 360;

        public Vector2Int GetRotationOffset(Dir dir) => dir switch
        {
            Dir.Left => new Vector2Int(0, width),
            Dir.Up => new Vector2Int(width, height),
            Dir.Right => new Vector2Int(height, 0),
            _ => Vector2Int.zero, // Dir.Down
        };

        public List<Vector2Int> GetGridPositionList(Vector2Int offset, Dir dir)
        {
            var gridPositionList = new List<Vector2Int>();
            int primary = GetWidth(dir);
            int secondary = GetHeight(dir);

            for (int x = 0; x < primary; x++)
            {
                for (int y = 0; y < secondary; y++)
                {
                    gridPositionList.Add(offset + new Vector2Int(x, y));
                }
            }

            return gridPositionList;
        }

        public int GetWidth(Dir dir) => dir is Dir.Down or Dir.Up ? width : height;

        public int GetHeight(Dir dir) => dir is Dir.Down or Dir.Up ? height : width;

        public CellType GetCellType() => cellType;
    }
}