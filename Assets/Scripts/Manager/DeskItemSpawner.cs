using UnityEngine;

/// <summary>
/// Spawns a fixed set of plates/cups onto desk cells once at game start, snapped to cell center
/// via GridPlaceable.PlaceAt.
/// </summary>
public class DeskItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cupPrefab;
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private Vector2Int[] cupSpawnCells;
    [SerializeField] private Vector2Int[] plateSpawnCells;

    private void Start()
    {
        SpawnAll(cupPrefab, cupSpawnCells);
        SpawnAll(platePrefab, plateSpawnCells);
    }

    private void SpawnAll(GameObject prefab, Vector2Int[] cells)
    {
        if (prefab == null || cells == null) return;

        GridSystem grid = GridSystem.Instance;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];
            GameObject instance = Instantiate(prefab, grid.CellToWorld(cell), Quaternion.identity);

            if (!instance.GetComponent<GridPlaceable>().PlaceAt(cell))
            {
                Debug.LogWarning($"[DeskItemSpawner] Cell {cell} already occupied — couldn't place {prefab.name}.");
                Destroy(instance);
            }
        }
    }
}
