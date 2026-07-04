using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Customer))]
public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private const float ArriveEpsilon = 0.01f;

    private Customer customer;
    private Seat reservedSeat;
    private List<Vector3> waypoints;
    private int waypointIndex;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    private void Start()
    {
        if (!SeatManager.Instance.TryReserveNearestSeat(transform.position, out Seat seat))
        {
            Debug.LogWarning("앉을 수 있는 좌석 없음");
            return;
        }

        reservedSeat = seat;

        List<Vector2Int> path = AStarPathfinder.FindPath(GridSystem.Instance.WorldToCell(transform.position), seat.Cell);
        if (path == null)
        {
            Debug.LogWarning("예약한 자리까지 경로 없음");
            reservedSeat.Release();
            reservedSeat = null;
            return;
        }

        waypoints = new List<Vector3>(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            waypoints.Add(GridSystem.Instance.CellToWorld(path[i]));
        }
        waypointIndex = 0;

        customer.ChangeState(CustomerState.Moving);
    }

    private void OnEnable()
    {
        if (customer == null) customer = GetComponent<Customer>();
        customer.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        customer.OnStateChanged -= HandleStateChanged;
    }

    private void Update()
    {
        if (waypoints == null) return;

        float z = transform.position.z;
        Vector3 target = waypoints[waypointIndex];
        target.z = z;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude > ArriveEpsilon * ArriveEpsilon) return;

        waypointIndex++;
        if (waypointIndex < waypoints.Count) return;

        Vector3 sitPosition = reservedSeat.SitWorldPosition;
        sitPosition.z = z;
        transform.position = sitPosition;

        waypoints = null;
        customer.Seat();
    }

    private void HandleStateChanged(CustomerState newState)
    {
        if (newState != CustomerState.LeavingSuccess && newState != CustomerState.LeavingFailure) return;
        if (reservedSeat == null) return;

        reservedSeat.Release();
        reservedSeat = null;
    }
}
