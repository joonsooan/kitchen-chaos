using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "KitchenChaos/Map Data")]
public class MapData : ScriptableObject
{
    [System.Serializable]
    public struct TileOverride
    {
        public Vector2Int cell;
        public TileType type;
    }

    [System.Serializable]
    public struct MonsterSpawn
    {
        public Vector2Int cell;
        public GameObject prefab; // Optional: left null until a designer assigns one.
    }

    [SerializeField] private int width = 12;
    [SerializeField] private int height = 8;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private TileType defaultTile = TileType.Floor;
    [SerializeField] private List<TileOverride> tileOverrides = new();
    [SerializeField] private Vector2Int playerSpawn = new(1, 1);
    [SerializeField] private List<MonsterSpawn> monsterSpawns = new();

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public TileType DefaultTile => defaultTile;
    public Vector2Int PlayerSpawn => playerSpawn;
    public int MonsterSpawnCount => monsterSpawns.Count;

    public MonsterSpawn GetMonsterSpawn(int index) => monsterSpawns[index];

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    /// <summary>Tile type for a cell: last matching override wins, out-of-bounds overrides are ignored.</summary>
    public TileType GetTile(Vector2Int cell)
    {
        TileType result = defaultTile;
        for (int i = 0; i < tileOverrides.Count; i++)
        {
            TileOverride entry = tileOverrides[i];
            if (!IsInBounds(entry.cell)) continue;
            if (entry.cell == cell) result = entry.type;
        }
        return result;
    }

    /// <summary>
    /// Single source of paint logic (the editor calls this). Painting a cell back to the
    /// default tile removes its override entirely; otherwise an existing override for that
    /// cell is updated in place or a new one is appended.
    /// </summary>
    public void SetTile(Vector2Int cell, TileType type)
    {
        if (!IsInBounds(cell)) return;

        if (type == defaultTile)
        {
            for (int i = tileOverrides.Count - 1; i >= 0; i--)
            {
                if (tileOverrides[i].cell == cell) tileOverrides.RemoveAt(i);
            }
            return;
        }

        for (int i = 0; i < tileOverrides.Count; i++)
        {
            if (tileOverrides[i].cell == cell)
            {
                TileOverride updated = tileOverrides[i];
                updated.type = type;
                tileOverrides[i] = updated;
                return;
            }
        }

        tileOverrides.Add(new TileOverride { cell = cell, type = type });
    }

    public void SetPlayerSpawn(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return;
        playerSpawn = cell;
    }

    /// <summary>Adds a monster spawn at cell (prefab left unassigned) if absent, removes it if already present.</summary>
    public void ToggleMonsterSpawn(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return;

        for (int i = 0; i < monsterSpawns.Count; i++)
        {
            if (monsterSpawns[i].cell == cell)
            {
                monsterSpawns.RemoveAt(i);
                return;
            }
        }

        monsterSpawns.Add(new MonsterSpawn { cell = cell, prefab = null });
    }

    private void OnValidate()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (cellSize < 0.01f) cellSize = 0.01f;

        playerSpawn = new Vector2Int(
            Mathf.Clamp(playerSpawn.x, 0, width - 1),
            Mathf.Clamp(playerSpawn.y, 0, height - 1));
    }
}
