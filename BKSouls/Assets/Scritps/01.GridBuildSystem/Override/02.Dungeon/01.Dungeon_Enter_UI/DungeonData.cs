using System.Collections.Generic;
using System.Text;
using BK;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Info/DungeonData ")]
public class DungeonData : ScriptableObject
{
    public enum Difficulty
    {
        [InspectorName("평화로움")]
        Easy = 0,
    
        [InspectorName("보통")]
        Normal = 1,
    
        [InspectorName("어려움")]
        Hard = 2,
    
        [InspectorName("전문가")]
        Expert = 3,
    
        [InspectorName("지옥")]
        Hell = 4,
    }
    
    [Header("Dungeon Information")] 
    public int dungeonID;
    public int dungeonKey;
    public Difficulty difficulty;
    public string dungeonSceneName;
    public string dungeonName;

    public List<int> enemyList;
    public List<int> mainResourceList;
    public int bossSpawnTimer ;

    public AudioSource bgm;
    
    public string GetFormattedInfo()
    {
        StringBuilder info = new StringBuilder();
    
        info.AppendLine($"[난이도 : {difficulty}]");
        info.AppendLine();
        info.AppendLine($"핵심 자원 : {GetResourceNames()}");
        info.AppendLine();
        info.AppendLine($"던전 폐쇄 시간 : {bossSpawnTimer}분");
        info.AppendLine();
    
        return info.ToString();
    }

    private string GetResourceNames()
    {
        StringBuilder info = new StringBuilder();
        List<string> resourceName = new List<string>();
        foreach (int itemID in mainResourceList)
        {
            var itemInfo = WorldItemDatabase.Instance.GetItemByID(itemID);
            if (!string.IsNullOrEmpty(itemInfo.itemName))
                resourceName.Add(itemInfo.itemName);
        }
        info.AppendLine(string.Join(", ", resourceName));
        return info.ToString();
    }
    
}
