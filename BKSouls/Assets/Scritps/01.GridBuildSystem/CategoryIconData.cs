using UnityEngine;

[CreateAssetMenu(fileName = "CategoryIconData", menuName = "Grid Build UI/Category Icon")]
public class CategoryIconData : ScriptableObject
{
    public CellType cellType;
    public Sprite cellIcon;
}
