using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[SelectionBase] // clicking a child sprite (e.g. "Visual") in the Scene view selects THIS root, so dragging moves the snapped pivot
public class GridSnap : MonoBehaviour
{
    private void Start()
    {
        // Play mode: snap once on spawn, then leave the object alone per-frame.
        if (Application.isPlaying)
        {
            SnapNow();
        }
    }

    private void Update()
    {
        // Play mode already snapped once in Start(); nothing to do per-frame.
        if (Application.isPlaying) return;

        // Edit mode: re-snap only when the designer actually moved the object.
        if (transform.hasChanged && GridSystem.Instance != null)
        {
            SnapNow();
            transform.hasChanged = false;
        }
    }

    /// <summary>
    /// Snaps this transform to the nearest grid cell center. Safe to call
    /// even if no GridSystem exists in the scene (no-op in that case).
    /// </summary>
    public void SnapNow()
    {
        GridSystem gridSystem = GridSystem.Instance;
        if (gridSystem == null) return;

        transform.position = gridSystem.Snap(transform.position);
    }
}
