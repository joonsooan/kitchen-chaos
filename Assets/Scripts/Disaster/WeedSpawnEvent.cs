using UnityEngine;

public class WeedSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject weedPrefabA;
    [SerializeField] private GameObject weedPrefabB;
    [SerializeField] private int weedCount = 3;
    [SerializeField] private float duration = 15f;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(0.8f, 1.2f);

    public override float Duration => duration;

    protected override bool TryTrigger()
    {
        if ((weedPrefabA == null && weedPrefabB == null) || weedCount <= 0) return false;

        GridSystem grid = GridSystem.Instance;
        if (grid == null) return false;

        for (int i = 0; i < weedCount; i++)
        {
            Vector2Int cell = grid.GetRandomWalkableCell();
            if (!grid.IsWalkable(cell)) continue;

            GameObject weed = Instantiate(GetRandomWeedPrefab(), grid.CellToWorld(cell), Quaternion.identity);
            weed.transform.localScale *= Random.Range(randomScaleRange.x, randomScaleRange.y);
            WeedTile weedTile = weed.GetComponent<WeedTile>();
            if (weedTile != null) weedTile.Init(cell, duration);
        }

        return true;
    }

    private GameObject GetRandomWeedPrefab()
    {
        if (weedPrefabA == null) return weedPrefabB;
        if (weedPrefabB == null) return weedPrefabA;
        return Random.value < 0.5f ? weedPrefabA : weedPrefabB;
    }
}
