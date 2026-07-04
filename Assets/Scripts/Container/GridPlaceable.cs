using UnityEngine;

public abstract class GridPlaceable : MonoBehaviour
{
    private GridSystem grid;
    private Collider2D col;
    private Vector2Int cell;
    private bool placed;

    protected virtual void Awake()
    {
        grid = GridSystem.Instance;
        col = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        if (!placed) PlaceAt(grid.WorldToCell(transform.position));
    }

    public bool PlaceAt(Vector2Int targetCell)
    {
        if (!grid.SetPlacedItem(targetCell, gameObject)) return false;

        cell = targetCell;
        transform.SetParent(null);
        transform.position = grid.CellToWorld(cell);
        placed = true;
        if (col != null) col.enabled = true;
        return true;
    }

    public void PrepareForHold()
    {
        ClearPlacement();
        if (col != null) col.enabled = false;
    }

    private void ClearPlacement()
    {
        if (!placed) return;
        grid.ClearPlacedItem(cell, gameObject);
        placed = false;
    }

    protected virtual void OnDisable()
    {
        ClearPlacement();
    }
}
