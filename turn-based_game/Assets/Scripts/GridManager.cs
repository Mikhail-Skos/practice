using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public GridCell[,] grid = new GridCell[7, 7];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGrid()
    {
        GridCell[] allCells = FindObjectsOfType<GridCell>();
        foreach (GridCell cell in allCells)
        {
            if (cell.gridPosition.x >= 0 && cell.gridPosition.x < 7 &&
                cell.gridPosition.y >= 0 && cell.gridPosition.y < 7)
            {
                grid[cell.gridPosition.x, cell.gridPosition.y] = cell;
            }
        }
    }

    public GridCell GetCell(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= 7 || pos.y < 0 || pos.y >= 7) return null;
        return grid[pos.x, pos.y];
    }

    public Hero GetHeroAtPosition(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        return cell != null ? cell.occupiedHero : null;
    }

    public bool IsCellEmpty(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        return cell != null && cell.occupiedHero == null;
    }

    public void ResetHighlights()
    {
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                GridCell cell = GetCell(new Vector2Int(x, y));
                if (cell != null) cell.ResetHighlight();
            }
        }
    }
}