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
    private bool isLeaving;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    public void BeginSeating()
    {
        isLeaving = false;
        reservedSeat = null;
        waypoints = null;

        if (!SeatManager.Instance.TryReserveNearestSeat(transform.position, out Seat seat))
        {
            customer.ReturnToPool();
            return;
        }

        reservedSeat = seat;

        Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);
        if (!TryBuildPath(startCell, seat.Cell))
        {
            reservedSeat.Release();
            reservedSeat = null;
            customer.ReturnToPool();
            return;
        }

        customer.ChangeState(CustomerState.Moving);
    }

    private bool TryBuildPath(Vector2Int fromCell, Vector2Int toCell)
    {
        List<Vector2Int> path = AStarPathfinder.FindPath(fromCell, toCell);
        if (path == null) return false;

        waypoints = new List<Vector3>(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            waypoints.Add(GridSystem.Instance.CellToWorld(path[i]));
        }
        waypointIndex = 0;
        return true;
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

        waypoints = null;

        if (isLeaving)
        {
            customer.ReturnToPool();
            return;
        }

        Vector3 sitPosition = reservedSeat.SitWorldPosition;
        sitPosition.z = z;
        transform.position = sitPosition;

        customer.Seat();
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.DrawSphere(waypoints[i], 0.1f);
        }

        Gizmos.color = Color.gray;
        for (int i = 0; i < waypointIndex; i++)
        {
            Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, waypoints[waypointIndex]);
        for (int i = waypointIndex; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
        }
    }

    private void HandleStateChanged(CustomerState newState)
    {
        if (newState != CustomerState.LeavingSuccess && newState != CustomerState.LeavingFailure) return;

        if (reservedSeat != null)
        {
            reservedSeat.Release();
            reservedSeat = null;
        }

        BeginLeaving();
    }

    private void BeginLeaving()
    {
        isLeaving = true;

        Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);
        Vector2Int spawnCell = CustomerSpawner.Instance.SpawnCell;

        if (!TryBuildPath(startCell, spawnCell))
        {
            customer.ReturnToPool();
        }
    }
}
