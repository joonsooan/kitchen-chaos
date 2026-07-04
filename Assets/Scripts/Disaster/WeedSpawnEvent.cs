using UnityEngine;

public class WeedSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject weedPrefab;
    [SerializeField] private int weedCount = 3;
    [SerializeField] private float duration = 15f;

    public override float Duration => duration;

    protected override bool TryTrigger()
    {
        if (weedPrefab == null || weedCount <= 0) return false;

        GridSystem grid = GridSystem.Instance;
        if (grid == null) return false;

        for (int i = 0; i < weedCount; i++)
        {
            Vector2Int cell = grid.GetRandomWalkableCell();
            if (!grid.IsWalkable(cell)) continue;

            GameObject weed = Instantiate(weedPrefab, grid.CellToWorld(cell), Quaternion.identity);
            WeedTile weedTile = weed.GetComponent<WeedTile>();
            if (weedTile != null) weedTile.Init(cell, duration);
        }

        return true;
    }
}
