using UnityEngine;

public class CabbageMonsterSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Vector2Int[] spawnCells;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 1f;   // 양상추 뽑기 시 스폰 확률(창 활성 중)
    [SerializeField] private float duration = 20f;

    private float windowEndTime = -1f;   // 이 ElapsedTime까지 뽑기 스폰 확률 활성

    public override float Duration => duration;

    // 재앙 발동: 스폰 확률 창만 켠다(직접 스폰 X). 팝업은 Trigger→OnAnyDisasterTriggered로 표시.
    protected override bool TryTrigger()
    {
        float now = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        windowEndTime = now + duration;
        return true;
    }

    // 양상추 뽑기(빌딩 트리거): 창 활성 중이면 spawnChance로 고정 셀에 스폰. 창 밖/평소 = 확률 0.
    public override void OnBuildingInteract(PlayerController player)
    {
        float now = GameManager.Instance != null ? GameManager.Instance.ElapsedTime : 0f;
        if (now > windowEndTime) return;            // 재앙 종료/평소 = 0 (자동)
        if (Random.value > spawnChance) return;     // 0.7
        SpawnMonster();
    }

    private void SpawnMonster()
    {
        if (monsterPrefab == null || spawnCells == null || spawnCells.Length == 0) return;

        Vector2Int cell = spawnCells[Random.Range(0, spawnCells.Length)];
        Vector3 spawnPosition = GridSystem.Instance.CellToWorld(cell);
        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        CabbageMonster cabbageMonster = monster.GetComponent<CabbageMonster>();
        if (cabbageMonster != null) cabbageMonster.Init(duration);
    }
}
