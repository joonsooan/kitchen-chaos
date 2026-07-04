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
        if (SeatManager.Instance != null) SeatManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // 씬 언로드 시 SeatManager가 먼저 파괴되면 Instance가 null일 수 있다
        if (SeatManager.Instance != null) SeatManager.Instance.Unregister(this);
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
