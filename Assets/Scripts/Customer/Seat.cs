using UnityEngine;

public class Seat : MonoBehaviour
{
    private Vector2Int cell;
    private Vector3 sitWorldPosition;

    public bool IsOccupied { get; private set; }

    public Vector2Int Cell => cell;
    public Vector3 SitWorldPosition => sitWorldPosition;

    private void Awake()
    {
        cell = GridSystem.Instance.WorldToCell(transform.position);
        sitWorldPosition = GridSystem.Instance.CellToWorld(cell);
    }

    private void OnEnable()
    {
        SeatManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        SeatManager.Instance.Unregister(this);
    }

    public bool TryReserve()
    {
        if (IsOccupied) return false;
        IsOccupied = true;
        return true;
    }

    public void Release()
    {
        IsOccupied = false;
    }
}
