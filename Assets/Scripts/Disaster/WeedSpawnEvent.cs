using UnityEngine;

public class WeedSpawnEvent : DisasterEvent
{
    [SerializeField] private GameObject weedPrefab;
    [SerializeField] private int weedCount = 3;
    [SerializeField] private float duration = 15f;

    public override void Trigger()
    {
        if (weedPrefab == null || weedCount <= 0) return;

        GridSystem grid = GridSystem.Instance;
        if (grid == null) return;

        for (int i = 0; i < weedCount; i++)
        {
            Vector2Int cell = grid.GetRandomWalkableCell();
            if (!grid.IsWalkable(cell)) continue;

            GameObject weed = Instantiate(weedPrefab, grid.CellToWorld(cell), Quaternion.identity);
            WeedTile weedTile = weed.GetComponent<WeedTile>();
            if (weedTile != null) weedTile.Init(cell, duration);
        }
    }
}
