using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CabbageMonster))]
public class CabbageMonsterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float waitDurationAtTarget = 2f;

    private const float ArriveEpsilon = 0.01f;

    private List<Vector3> waypoints;
    private int waypointIndex;
    private float waitTimer;

    private void OnEnable()
    {
        waypoints = null;
        waitTimer = 0f;
        TryPickNewTarget();
    }

    private void Update()
    {
        if (waypoints == null)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) TryPickNewTarget();
            return;
        }

        float z = transform.position.z;
        Vector3 target = waypoints[waypointIndex];
        target.z = z;

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude > ArriveEpsilon * ArriveEpsilon) return;

        waypointIndex++;
        if (waypointIndex < waypoints.Count) return;

        waypoints = null;
        waitTimer = waitDurationAtTarget;
    }

    private void TryPickNewTarget()
    {
        GridSystem grid = GridSystem.Instance;
        Vector2Int startCell = grid.WorldToCell(transform.position);
        Vector2Int targetCell = grid.GetRandomWalkableCell();

        List<Vector2Int> path = AStarPathfinder.FindPath(startCell, targetCell);
        if (path == null || path.Count == 0)
        {
            waitTimer = waitDurationAtTarget;
            return;
        }

        waypoints = new List<Vector3>(path.Count);
        for (int i = 0; i < path.Count; i++)
        {
            waypoints.Add(grid.CellToWorld(path[i]));
        }
        waypointIndex = 0;
    }
}
