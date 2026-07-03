using UnityEngine;

/// <summary>
/// Scales this object's sprite so it covers its Building footprint's bounding box in
/// grid cells, following MapData/GridSystem cellSize automatically. Building/BuildingData
/// are only READ here - never modified. Without a Building (or footprint) the size falls
/// back to a single cell.
///
/// Anchor convention (project-wide): footprint offsets grow +x/+y from (0,0), GridSnap
/// keeps the pivot on the anchor cell's center, and the ART's import pivot must sit at
/// the center of its first cell - (0.5/N, 0.5/M) for an N x M-cell sprite (1x1 art keeps
/// the default Center pivot). With that pivot the sprite lines up with cell boundaries
/// using nothing but this transform scale - no child objects, no code offsets. A
/// BoxCollider2D on the same object is auto-fitted to the sprite (pivot-aware) and scales
/// with the transform.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class GridSized : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Building building;

    // Edit-mode change detection: reapply only when one of these no longer matches reality.
    private bool pendingReapply;
    private float lastCellSize;
    private Vector2 lastRectSize;
    private Sprite lastSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (building == null) building = GetComponent<Building>();

        GridSystem grid = GridSystem.Instance;
        if (grid == null || spriteRenderer == null) return;

        float currentCellSize = grid.CellSize;
        Sprite currentSprite = spriteRenderer.sprite;
        Vector2 rectSize = GetFootprintRectSize();

        bool changed = pendingReapply
            || currentCellSize != lastCellSize
            || rectSize != lastRectSize
            || currentSprite != lastSprite;

        if (!changed) return;

        ApplySize();

        lastCellSize = currentCellSize;
        lastRectSize = rectSize;
        lastSprite = currentSprite;
        pendingReapply = false;
    }

    /// <summary>Scales the sprite to the footprint bounding box and fits the collider to it.</summary>
    public void ApplySize()
    {
        GridSystem grid = GridSystem.Instance;
        if (grid == null || spriteRenderer == null || spriteRenderer.sprite == null) return;

        Vector2 native = spriteRenderer.sprite.bounds.size; // world size at scale 1, PPU-aware
        if (native.x <= 0f || native.y <= 0f) return;

        Vector2 target = GetFootprintRectSize() * grid.CellSize;
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(target.x / native.x, target.y / native.y, scale.z);

        // Pivot-aware local fit: sprite.bounds.center reflects the custom pivot, so the
        // collider covers exactly what the sprite shows once the transform scale applies.
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = native;
            box.offset = spriteRenderer.sprite.bounds.center;
        }
    }

    /// <summary>Footprint bounding-box size in cells; a single cell when there is no footprint.</summary>
    private Vector2 GetFootprintRectSize()
    {
        if (building != null)
        {
            Vector2Int[] cells = building.FootprintCells;
            if (cells != null && cells.Length > 0)
            {
                Vector2Int min = cells[0];
                Vector2Int max = cells[0];
                for (int i = 1; i < cells.Length; i++)
                {
                    min = Vector2Int.Min(min, cells[i]);
                    max = Vector2Int.Max(max, cells[i]);
                }
                return max - min + Vector2Int.one;
            }
        }

        return Vector2.one;
    }

    private void OnValidate()
    {
        pendingReapply = true;
    }
}
