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

        ReturnStation station = FindStationFor(carried.ContainerType);
        Vector2Int startCell = GridSystem.Instance.WorldToCell(transform.position);
        Vector2Int returnCell = GridSystem.Instance.WorldToCell(station.transform.position) + Vector2Int.down;

        List<Vector2Int> path = AStarPathfinder.FindPath(startCell, returnCell);
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
