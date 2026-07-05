using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Post-serve customer pipeline: takes the served container from TableServing, eats in
/// place for a success/failure-specific duration, then carries the dish along an A* path
/// to the cell below the matching ReturnStation (cup/plate), restocks it, and hands
/// control back to the normal leaving flow via the Leaving state change.
/// </summary>
[RequireComponent(typeof(Customer))]
[DisallowMultipleComponent]
public class CustomerDishReturn : MonoBehaviour
{
    [SerializeField] private float successEatSeconds = 3f;
    [SerializeField] private float failureEatSeconds = 1.5f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector3 carryOffset = new Vector3(0f, 0.9f, -0.01f);
    [SerializeField] private float retryInterval = 2f;                          // 반납길이 막혔을 때 재탐색 간격
    [SerializeField] private Vector3 noticeOffset = new Vector3(0f, 1.4f, 0f);   // "반납 불가" 안내 위치(머리 위)

    private const float ArriveEpsilon = 0.01f;

    private Customer customer;
    private ContainerKitchenObject carried;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    // Called by TableServing; takes ownership of the container's world object.
    public void Begin(ContainerKitchenObject container, bool isCorrect)
    {
        carried = container;
        carried.transform.SetParent(transform, false);
        carried.transform.localPosition = carryOffset;

        // Idle parks the customer: the tolerance timer only ticks in Waiting, and
        // TableServing only matches Waiting customers, so eating can't time out
        // and can't receive a second dish.
        customer.ChangeState(CustomerState.Idle);

        StartCoroutine(EatAndReturn(isCorrect));
    }

    private IEnumerator EatAndReturn(bool isCorrect)
    {
        yield return new WaitForSeconds(isCorrect ? successEatSeconds : failureEatSeconds);

        // 다 먹었으니 손에 든 완성 요리를 빈 그릇으로 — 빈 접시/컵을 들고 반납하러 간다.
        carried.Empty();

        ReturnStation station = FindStationFor(carried.ContainerType);
        Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);
        Vector2Int returnCell = GridSystem.Instance.WorldToCell(station.transform.position) + Vector2Int.down;

        List<Vector2Int> path = AStarPathfinder.FindPath(startCell, returnCell, avoidOccupants: true);

        // 경로 없음 = 잡초 등 점유로 통로가 막힘. 머리 위 안내를 띄우고 retryInterval초마다
        // 재탐색하며, 플레이어가 길을 뚫어줄 때까지(또는 잡초가 스스로 사라질 때까지) 제자리 대기.
        while (path == null)
        {
            ShowBlockedNotice();
            yield return new WaitForSeconds(retryInterval);
            startCell = GridSystem.Instance.WorldToCell(transform.position);
            path = AStarPathfinder.FindPath(startCell, returnCell, avoidOccupants: true);
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 target = GridSystem.Instance.CellToWorld(path[i]);
            target.z = transform.position.z;
            while ((transform.position - target).sqrMagnitude > ArriveEpsilon * ArriveEpsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        station.AcceptReturn();
        Destroy(carried.gameObject);
        carried = null;

        customer.ChangeState(isCorrect ? CustomerState.LeavingSuccess : CustomerState.LeavingFailure);
    }

    // 반납 통로가 막혔을 때 손님 머리 위에 검은 글씨로 안내 (2초 간격 재탐색마다 재노출).
    private void ShowBlockedNotice()
    {
        var prefab = Resources.Load<GameObject>("UI/World/ServeResultPopup");
        if (prefab == null) return;

        string message = UnityEngine.Random.value < 0.5f ? "다 먹었어용.." : "배불러용..";
        Instantiate(prefab).GetComponent<ServeResultPopup>()
            .Show(transform.position + noticeOffset, message, Color.black);
    }

    private static ReturnStation FindStationFor(CarryingItemType containerType)
    {
        ReturnStation[] stations = FindObjectsByType<ReturnStation>(FindObjectsSortMode.None);
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i].ContainerType == containerType) return stations[i];
        }
        return null;
    }

    // Pool hygiene: the coroutine dies with SetActive(false), so drop any dish still
    // carried before this customer object is reused.
    private void OnDisable()
    {
        if (carried == null) return;
        Destroy(carried.gameObject);
        carried = null;
    }
}
