using UnityEngine;

public class CabbageMonsterSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Vector2Int[] spawnCells;

    public override void Trigger()
    {
        if (monsterPrefab == null || spawnCells == null || spawnCells.Length == 0) return;

        Vector2Int cell = spawnCells[Random.Range(0, spawnCells.Length)];
        Vector3 spawnPosition = GridSystem.Instance.CellToWorld(cell);
        Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
    }
}
