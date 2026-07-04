using UnityEngine;

public class Seat : MonoBehaviour
{
    // 좌석이 빌 때마다 알림 — CustomerSpawner가 대기 중인 손님을 바로 입장시키는 데 사용.
    public static event System.Action OnSeatReleased;

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
        OnSeatReleased?.Invoke();
    }
}
