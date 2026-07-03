using UnityEngine;

/// <summary>
/// Runtime-only occupancy registration for objects placed on the grid (props, enemies, etc.).
/// One object occupies exactly one cell; multi-cell footprints are a future extension.
/// </summary>
public class GridOccupant : MonoBehaviour
{
    private GridSystem grid;
    private Vector2Int cell;

    private void Start()
    {
        grid = GridSystem.Instance;
        if (grid == null) return;

        cell = grid.WorldToCell(transform.position);
        grid.SetOccupant(cell, gameObject);
    }

    private void OnDisable()
    {
        // grid may already be destroyed during scene teardown - the null check
        // (Unity's overridden ==) covers both "never found" and "already destroyed".
        if (grid == null) return;
        grid.ClearOccupant(cell, gameObject);
    }
}
