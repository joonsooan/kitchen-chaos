using UnityEngine;

public class CabbageMonsterSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Vector2Int[] spawnCells;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 1f;
    [SerializeField] private float duration = 20f;

    public override float Duration => duration;
    public override bool ShowsPopup => false;

    protected override bool TryTrigger()
    {
        if (monsterPrefab == null || spawnCells == null || spawnCells.Length == 0) return false;
        if (Random.value > spawnChance) return false;

        Vector2Int cell = spawnCells[Random.Range(0, spawnCells.Length)];
        Vector3 spawnPosition = GridSystem.Instance.CellToWorld(cell);
        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        CabbageMonster cabbageMonster = monster.GetComponent<CabbageMonster>();
        if (cabbageMonster != null) cabbageMonster.Init(duration);
        return true;
    }
}
