using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class GridBuildingUI : MonoBehaviour
{
    private int _gridWidth;
    private int _gridHeight;

    private int _entrancePosition;
    private Dir _exitDirection ;
    private int _exitPosition;
    
    [Header("UI References")]
    public GameObject gridCellPrefab; // 기본 그리드 셀 프리팹
    public GameObject arrowPrefab;    // 화살표 프리팹
    public Transform gridParent;      // 그리드의 부모 Transform
    
    [Header("Styling")]
    [SerializeField] private Color gridCellColor = Color.white;
    [SerializeField] private  Color entranceArrowColor = Color.green;
    [SerializeField] private  Color exitArrowColor = Color.red;
    [SerializeField] private  float cellSize = 50f;
    [SerializeField] private  float arrowSize = 40f;
    
    private GridLayoutGroup _gridLayoutGroup;
    private readonly List<GameObject> _gridCells = new List<GameObject>();
    private readonly List<GameObject> _arrows = new List<GameObject>();
    
    public void SetGridLayer(int x, int y, int entrancePos, Dir exitDir, int exitPos)
    {
        _gridWidth = x;
        _gridHeight = y;
        _entrancePosition = entrancePos;
        _exitDirection = exitDir;
        _exitPosition = exitPos;
        
        SetupGridLayout();
        GenerateGrid();
    }
    
    private void SetupGridLayout()
    {
        // GridLayoutGroup 설정
        _gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
        if (_gridLayoutGroup == null)
        {
            _gridLayoutGroup = gridParent.gameObject.AddComponent<GridLayoutGroup>();
        }
        
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _gridWidth;
        _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        _gridLayoutGroup.spacing = new Vector2(2f, 2f);
        _gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        _gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        _gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
    }
    
    private void GenerateGrid()
    {
        ClearGrid();
        CreateGridCells();
        CreateArrows();
    }
    
    public void ClearGrid()
    {
        // 기존 그리드 셀들 제거
        foreach (GameObject cell in _gridCells)
        {
            if (cell != null)
                DestroyImmediate(cell);
        }
        _gridCells.Clear();
        
        // 기존 화살표들 제거
        foreach (GameObject arrow in _arrows)
        {
            if (arrow != null)
                DestroyImmediate(arrow);
        }
        _arrows.Clear();
    }
    
    private void CreateGridCells()
    {
        for (int i = 0; i < _gridWidth * _gridHeight; i++)
        {
            GameObject cell = Instantiate(gridCellPrefab, gridParent);
            
            // 셀 색상 설정
            Image cellImage = cell.GetComponent<Image>();
            if (cellImage != null)
            {
                cellImage.color = gridCellColor;
            }
            
            _gridCells.Add(cell);
        }
    }
    
    private void CreateArrows()
    {
        CreateEntranceArrow();
        CreateExitArrow();
    }
    
    private void CreateEntranceArrow()
    {
        // 입구 화살표 (아래줄에서 위쪽을 향하는 화살표)
        Vector3 entrancePos = GetEntranceArrowPosition();
        GameObject entranceArrow = Instantiate(arrowPrefab, transform);
        
        // 화살표 위치 설정
        RectTransform arrowRect = entranceArrow.GetComponent<RectTransform>();
        arrowRect.anchoredPosition = entrancePos;
        arrowRect.sizeDelta = new Vector2(arrowSize, arrowSize);
        
        // 위쪽을 향하도록 회전 (입구)
        arrowRect.rotation = Quaternion.Euler(0, 0, 0);
        
        // 색상 설정
        Image arrowImage = entranceArrow.GetComponent<Image>();
        if (arrowImage != null)
        {
            arrowImage.color = entranceArrowColor;
        }
        
        _arrows.Add(entranceArrow);
    }
    
    private void CreateExitArrow()
    {
        // 출구 화살표
        Vector3 exitPos = GetExitArrowPosition();
        GameObject exitArrow = Instantiate(arrowPrefab, transform);
        
        // 화살표 위치 설정
        RectTransform arrowRect = exitArrow.GetComponent<RectTransform>();
        arrowRect.anchoredPosition = exitPos;
        arrowRect.sizeDelta = new Vector2(arrowSize, arrowSize);
        
        // 출구 방향에 따른 회전 설정
        float rotation = GetExitArrowRotation();
        arrowRect.rotation = Quaternion.Euler(0, 0, rotation);
        
        // 색상 설정
        Image arrowImage = exitArrow.GetComponent<Image>();
        if (arrowImage != null)
        {
            arrowImage.color = exitArrowColor;
        }
        
        _arrows.Add(exitArrow);
    }
    
    private Vector3 GetEntranceArrowPosition()
    {
        // 그리드의 아래쪽 중앙에서 입구 위치 계산
        float gridStartX = -(_gridWidth - 1) * (cellSize + 2f) * 0.5f;
        float gridBottomY = -(_gridHeight - 1) * (cellSize + 2f) * 0.5f;
        
        float entranceX = gridStartX + _entrancePosition * (cellSize + 2f);
        float entranceY = gridBottomY - cellSize - 10f; // 그리드 아래쪽에 위치
        
        return new Vector3(entranceX, entranceY, 0);
    }
    
    private Vector3 GetExitArrowPosition()
    {
        float gridStartX = -(_gridWidth - 1) * (cellSize + 2f) * 0.5f;
        float gridStartY = (_gridHeight - 1) * (cellSize + 2f) * 0.5f;
        float gridEndX = (_gridWidth - 1) * (cellSize + 2f) * 0.5f;
        float gridBottomY = -(_gridHeight - 1) * (cellSize + 2f) * 0.5f;
        
        Vector3 exitPos = Vector3.zero;
        
        switch (_exitDirection)
        {
            case Dir.Up:
                // 위쪽 벽: 우에서 좌로 갈수록 _exitPosition 증가 (회전 일관성)
                exitPos.x = gridEndX - _exitPosition * (cellSize + 2f);
                exitPos.y = gridStartY + cellSize + 10f;
                break;
            
            case Dir.Right:
                // 우측 벽: 아래에서 위로 갈수록 _exitPosition 증가 (회전 일관성)
                exitPos.x = gridEndX + cellSize + 10f;
                exitPos.y = gridBottomY + _exitPosition * (cellSize + 2f);
                break;
            
            case Dir.Down:
                // 아래쪽 벽: 좌에서 우로 갈수록 _exitPosition 증가
                exitPos.x = gridStartX + _exitPosition * (cellSize + 2f);
                exitPos.y = gridBottomY - cellSize - 10f;
                break;
            
            case Dir.Left:
                // 왼쪽 벽: 위에서 아래로 갈수록 _exitPosition 증가 (수정됨)
                exitPos.x = gridStartX - cellSize - 10f;
                exitPos.y = gridStartY - _exitPosition * (cellSize + 2f);
                break;
        }
        
        return exitPos;
    }
    
    private float GetExitArrowRotation()
    {
        switch (_exitDirection)
        {
            case Dir.Up:
                return 0f;      // 위쪽
            case Dir.Right:
                return -90f;    // 오른쪽
            case Dir.Down:
                return 180f;    // 아래쪽
            case Dir.Left:
                return 90f;     // 왼쪽
            default:
                return 0f;
        }
    }
}