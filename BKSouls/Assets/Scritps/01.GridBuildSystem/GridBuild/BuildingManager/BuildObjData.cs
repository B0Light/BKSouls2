using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

        [System.Serializable]
        public struct ItemCostEntry
        {
            public GridItem item;
            public int count;

            [SerializeField, HideInInspector, FormerlySerializedAs("itemId")]
            private int legacyItemId;

            public GridItem GetItem(bool allowLegacyResolve)
            {
                if (item != null)
                    return item;

                if (!allowLegacyResolve || legacyItemId <= 0 || WorldItemDatabase.Instance == null)
                    return null;

                return WorldItemDatabase.Instance.GetItemByID(legacyItemId) as GridItem;
            }
        }

        public Transform prefab;

        [Header("Build Cost")]
        [SerializeField] private List<ItemCostEntry> costItems = new List<ItemCostEntry>();
        public Dictionary<GridItem, int> costItemDic = new Dictionary<GridItem, int>();

        private void OnEnable()
        {
            RefreshCostItemDictionary(false);
        }

        public IReadOnlyDictionary<GridItem, int> GetCostItems()
        {
            RefreshCostItemDictionary(true);
            return costItemDic;
        }

        private void RefreshCostItemDictionary(bool allowLegacyResolve)
        {
            costItemDic = new Dictionary<GridItem, int>();
            foreach (var entry in costItems)
            {
                GridItem item = entry.GetItem(allowLegacyResolve);
                if (item != null && entry.count > 0)
                    costItemDic[item] = entry.count;
            }
        }

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
