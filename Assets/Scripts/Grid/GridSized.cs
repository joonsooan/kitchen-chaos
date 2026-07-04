using UnityEngine;

/// <summary>
/// Fits this object's BoxCollider2D to its Building footprint's bounding box in grid cells,
/// following MapData/GridSystem cellSize automatically. Building/BuildingData are only READ
/// here - never modified. Without a Building (or footprint) the size falls back to a single
/// cell. Purely collider/grid-interaction sizing - does NOT touch the sprite. Sprite visuals
/// live on a separate child object (e.g. "Visual") and are positioned/scaled freely by hand.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class GridSized : MonoBehaviour
{
    private Building building;

    // Edit-mode change detection: reapply only when one of these no longer matches reality.
    private bool pendingReapply;
    private float lastCellSize;
    private Vector2 lastRectSize;

    private void Awake()
    {
        building = GetComponent<Building>();
    }

    private void Start()
    {
        // Play mode: apply once on spawn. Edit mode reapplication is owned by Update below.
        if (Application.isPlaying)
        {
            ApplySize();
        }
    }

    private void Update()
    {
        if (Application.isPlaying) return;

        if (building == null) building = GetComponent<Building>();

        GridSystem grid = GridSystem.Instance;
        if (grid == null) return;

        float currentCellSize = grid.CellSize;
        Vector2 rectSize = GetFootprintRectSize();

        bool changed = pendingReapply
            || currentCellSize != lastCellSize
            || rectSize != lastRectSize;

        if (!changed) return;

        ApplySize();

        lastCellSize = currentCellSize;
        lastRectSize = rectSize;
        pendingReapply = false;
    }

    /// <summary>Fits the collider to the footprint bounding box, centered on the footprint (not the sprite).</summary>
    public void ApplySize()
    {
        GridSystem grid = GridSystem.Instance;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (grid == null || box == null) return;

        float cellSize = grid.CellSize;
        GetFootprintBounds(out Vector2Int min, out Vector2Int max);

        box.size = (Vector2)(max - min + Vector2Int.one) * cellSize;
        box.offset = (Vector2)(min + max) * 0.5f * cellSize;
    }

    /// <summary>Footprint bounding-box size in cells; a single cell when there is no footprint.</summary>
    private Vector2 GetFootprintRectSize()
    {
        GetFootprintBounds(out Vector2Int min, out Vector2Int max);
        return max - min + Vector2Int.one;
    }

    private void GetFootprintBounds(out Vector2Int min, out Vector2Int max)
    {
        if (building != null)
        {
            Vector2Int[] cells = building.FootprintCells;
            if (cells != null && cells.Length > 0)
            {
                min = cells[0];
                max = cells[0];
                for (int i = 1; i < cells.Length; i++)
                {
                    min = Vector2Int.Min(min, cells[i]);
                    max = Vector2Int.Max(max, cells[i]);
                }
                return;
            }
        }

        min = Vector2Int.zero;
        max = Vector2Int.zero;
    }

    private void OnValidate()
    {
        pendingReapply = true;
    }
}
