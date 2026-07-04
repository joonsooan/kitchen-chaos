using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    // buildingData 미지정 시 기본 1칸 점유 (구 GridOccupant 동작 흡수).
    // 명시적으로 빈 footprint 배열을 가진 데이터는 "점유 없음"(장식물)으로 해석한다.
    private static readonly Vector2Int[] DefaultFootprint = { Vector2Int.zero };

    [SerializeField] private BuildingData buildingData;

    private GridSystem grid;
    private readonly List<Vector2Int> occupiedCells = new();

    public BuildingData BuildingData => buildingData;

    public Vector2Int[] FootprintCells =>
        buildingData != null ? buildingData.footprintCells : DefaultFootprint;

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
        if (grid == null) return;

        Vector2Int anchorCell = grid.WorldToCell(transform.position);
        occupiedCells.Clear();

        foreach (Vector2Int offset in FootprintCells)
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
        GridSystem gridSystem = GridSystem.Instance;
        float cellSize = gridSystem != null ? gridSystem.CellSize : 1f;
        Vector2Int anchorCell = gridSystem != null ? gridSystem.WorldToCell(transform.position) : Vector2Int.zero;

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.4f);
        foreach (Vector2Int offset in FootprintCells)
        {
            Vector3 center = gridSystem != null
                ? gridSystem.CellToWorld(anchorCell + offset)
                : transform.position + new Vector3(offset.x, offset.y, 0f) * cellSize;
            Gizmos.DrawCube(center, Vector3.one * (cellSize * 0.9f));
        }
    }
}
