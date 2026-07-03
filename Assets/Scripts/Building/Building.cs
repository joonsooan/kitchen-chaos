using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;

    private GridSystem grid;
    private readonly List<Vector2Int> occupiedCells = new();

    public BuildingData BuildingData => buildingData;

    private void Start()
    {
        RegisterOccupancy();
    }

    private void OnDisable()
    {
        UnregisterOccupancy();
    }

    private void RegisterOccupancy()
    {
        grid = GridSystem.Instance;
        if (grid == null || buildingData == null) return;

        Vector2Int anchorCell = grid.WorldToCell(transform.position);
        occupiedCells.Clear();

        foreach (Vector2Int offset in buildingData.footprintCells)
        {
            Vector2Int cell = anchorCell + offset;
            if (grid.SetOccupant(cell, gameObject))
            {
                occupiedCells.Add(cell);
            }
        }
    }

    private void UnregisterOccupancy()
    {
        if (grid == null) return;

        foreach (Vector2Int cell in occupiedCells)
        {
            grid.ClearOccupant(cell, gameObject);
        }
        occupiedCells.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (buildingData == null) return;

        GridSystem gridSystem = GridSystem.Instance;
        float cellSize = gridSystem != null ? gridSystem.CellSize : 1f;
        Vector2Int anchorCell = gridSystem != null ? gridSystem.WorldToCell(transform.position) : Vector2Int.zero;

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.4f);
        foreach (Vector2Int offset in buildingData.footprintCells)
        {
            Vector3 center = gridSystem != null
                ? gridSystem.CellToWorld(anchorCell + offset)
                : transform.position + new Vector3(offset.x, offset.y, 0f) * cellSize;
            Gizmos.DrawCube(center, Vector3.one * (cellSize * 0.9f));
        }
    }
}
