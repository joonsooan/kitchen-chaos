using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Customer))]
public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float leaveRetryInterval = 2f;                          // 퇴장 경로 막힘 시 재탐색 간격
    [SerializeField] private Vector3 leaveNoticeOffset = new Vector3(0f, 1.4f, 0f);   // 퇴장 안내 위치(머리 위)

    private const float ArriveEpsilon = 0.01f;

    private Customer customer;
    private Seat reservedSeat;
    private List<Vector3> waypoints;
    private int waypointIndex;
    private bool isLeaving;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    // CustomerDishReturn처럼 이 컴포넌트를 거치지 않고 직접 transform을 옮기는 외부 이동 코드가
    // 매 프레임 이동 delta로 호출해서 FacingDirection을 갱신한다.
    public void ReportExternalMove(Vector2 delta)
    {
        if (delta.sqrMagnitude > 0f) FacingDirection = delta.normalized;
    }

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    public void BeginSeating()
    {
        isLeaving = false;
        reservedSeat = null;
        waypoints = null;

        Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);

        // 가까운 순으로 '실제 경로가 뚫린' 첫 좌석 예약. 가장 가까운 좌석이 막혀도
        // 도달 가능한 다른 좌석으로 입장 → 한 좌석 막혔다고 입장 전체가 멈추지 않게.
        foreach (Seat candidate in SeatManager.Instance.GetFreeSeatsByDistance(transform.position))
        {
            if (!TryBuildPath(startCell, candidate.Cell)) continue;   // 이 좌석 경로 막힘 → 다음 후보
            if (!candidate.TryReserve()) continue;                    // (동시 예약 방지)

            reservedSeat = candidate;
            customer.ChangeState(CustomerState.Moving);
            return;
        }

        // 도달 가능한 빈 좌석이 하나도 없음 → 안내 후 풀로 반환(다음 스폰/좌석 해제 때 재시도).
        CustomerSpawner.Instance.ShowSeatBlockedNotice();
        customer.ReturnToPool();
    }

    private bool TryBuildPath(Vector2Int fromCell, Vector2Int toCell)
    {
        List<Vector2Int> path = AStarPathfinder.FindPath(fromCell, toCell, avoidOccupants: true);
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

        Vector3 previousPosition = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        Vector2 delta = transform.position - previousPosition;
        if (delta.sqrMagnitude > 0f) FacingDirection = delta.normalized;

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

        FacingDirection = reservedSeat.IndexInTable switch
        {
            0 => Vector2.right,
            1 => Vector2.left,
            _ => FacingDirection
        };

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
        StartCoroutine(LeaveWhenPathClears());
    }

    // 퇴장 경로가 막혀 있으면 제자리에서 대기하며 재탐색 → 뚫리면 waypoints가 채워져
    // Update가 이어서 걸어 나간다. (반납 로직과 동일한 대기·재시도 방식)
    private IEnumerator LeaveWhenPathClears()
    {
        Vector2Int spawnCell = CustomerSpawner.Instance.SpawnCell;

        while (true)
        {
            Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);
            if (TryBuildPath(startCell, spawnCell)) yield break;

            ShowLeaveBlockedNotice();
            yield return new WaitForSeconds(leaveRetryInterval);
        }
    }

    // 퇴장 경로가 막혔을 때 손님 머리 위에 검은 글씨로 안내 (재탐색마다 재노출).
    private void ShowLeaveBlockedNotice()
    {
        var prefab = Resources.Load<GameObject>("UI/World/ServeResultPopup");
        if (prefab == null) return;

        string message = UnityEngine.Random.value < 0.5f ? "나갈래용.." : "내보내 줘..";
        Instantiate(prefab).GetComponent<ServeResultPopup>()
            .Show(transform.position + leaveNoticeOffset, message, Color.black);
    }
}
