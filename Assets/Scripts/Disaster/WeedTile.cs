using System.Collections;
using UnityEngine;

public class WeedTile : MonoBehaviour, IAttackable
{
    [SerializeField] private int hitsToRemove = 3;

    private GridSystem grid;
    private Vector2Int cell;
    private int hitsRemaining;
    private bool despawned;

    private void Awake()
    {
        hitsRemaining = hitsToRemove;
    }

    // lifetime = 스폰시킨 재난 이벤트의 duration 그대로 전달받음 (WeedTile 자체 필드 아님).
    public void Init(Vector2Int spawnCell, float lifetime)
    {
        cell = spawnCell;
        grid = GridSystem.Instance;
        grid.SetOccupant(cell, gameObject);
        StartCoroutine(LifetimeRoutine(lifetime));
    }

    public void Hit(PlayerController player)
    {
        hitsRemaining -= PowerBuff.OneShotActive ? hitsRemaining : 1;   // 파워 업 버프: 한 방
        if (hitsRemaining <= 0) Despawn();
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Despawn();
    }

    private void Despawn()
    {
        if (despawned) return;
        despawned = true;

        if (grid != null) grid.ClearOccupant(cell, gameObject);
        Destroy(gameObject);
    }
}
