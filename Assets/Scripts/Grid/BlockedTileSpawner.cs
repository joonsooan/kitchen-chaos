using UnityEngine;

public class BlockedTileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private Transform spawnParent;

    private void Start()
    {
        if (GridSystem.Instance == null || buildingPrefab == null) return;

        GridSystem gridSystem = GridSystem.Instance;
        Transform parent = spawnParent != null ? spawnParent : transform;

        for (int y = 0; y < gridSystem.Height; y++)
        {
            for (int x = 0; x < gridSystem.Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (gridSystem.GetTileType(cell) != TileType.Blocked) continue;

                Instantiate(buildingPrefab, gridSystem.CellToWorld(cell), Quaternion.identity, parent);
            }
        }
    }
}
