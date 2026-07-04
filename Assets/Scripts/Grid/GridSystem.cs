using UnityEngine;

public class GridSystem : MonoBehaviour
{
    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0)
    };

    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 8;
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private MapData mapData;

    // Runtime-only state, flat arrays indexed as y * Width + x.
    // Rebuilt whenever the effective Width*Height no longer matches the cached size
    // or the mapData reference itself changes (size alone can't detect an asset swap).
    private TileType[] baseTiles;
    private GameObject[] occupants;
    // Separate from occupants: occupants reserve a cell for furniture (blocks nothing on its own,
    // see GetWalkableNeighborsNonAlloc note), placedItems is what's sitting on top of a cell
    // (floor or desk) — an ingredient or a plate/cup. A desk (occupant) and an item placed on it
    // (placedItem) can coexist on the same cell since they're tracked independently.
    private GameObject[] placedItems;
    private MapData builtFromMap;

    private static GridSystem instance;
    public static GridSystem Instance
    {
        get
        {
            if (instance == null) instance = FindAnyObjectByType<GridSystem>();
            return instance;
        }
    }

    public MapData Map => mapData;

    // Effective grid values: mapData (if assigned) overrides the serialized fallback fields
    // below, which stay usable for scenes with no MapData asset. All internal math routes
    // through these three so mapData/no-mapData behave identically everywhere else.
    public float CellSize => mapData != null ? mapData.CellSize : cellSize;
    public int Width => mapData != null ? mapData.Width : width;
    public int Height => mapData != null ? mapData.Height : height;

    private void Awake()
    {
        instance = this;
        EnsureRuntimeState();
    }

    /// <summary>
    /// Converts a world position into a grid cell coordinate.
    /// Origin (transform.position) is the bottom-left corner of cell (0,0).
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 origin = transform.position;
        float size = CellSize;
        int cellX = Mathf.FloorToInt((worldPosition.x - origin.x) / size);
        int cellY = Mathf.FloorToInt((worldPosition.y - origin.y) / size);
        return new Vector2Int(cellX, cellY);
    }

    /// <summary>
    /// Converts a grid cell coordinate into the world position of its center.
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 origin = transform.position;
        float size = CellSize;
        float worldX = origin.x + (cell.x + 0.5f) * size;
        float worldY = origin.y + (cell.y + 0.5f) * size;
        return new Vector3(worldX, worldY, origin.z);
    }

    /// <summary>
    /// Snaps a world position to the nearest cell center, preserving the input z
    /// so objects keep their own depth for sprite sorting.
    /// </summary>
    public Vector3 Snap(Vector3 worldPosition)
    {
        Vector3 snapped = CellToWorld(WorldToCell(worldPosition));
        snapped.z = worldPosition.z;
        return snapped;
    }

    public bool IsInBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
    }

    /// <summary>Tile type for a cell. Out-of-bounds cells are treated as Blocked.</summary>
    public TileType GetTileType(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return TileType.Blocked;
        EnsureRuntimeState();
        return baseTiles[CellIndex(cell)];
    }

    /// <summary>True if the cell is in bounds, its tile isn't Blocked, and no occupant holds it.</summary>
    public bool IsWalkable(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return false;
        EnsureRuntimeState();
        int index = CellIndex(cell);
        return baseTiles[index] != TileType.Blocked && occupants[index] == null;
    }

    public bool TryGetOccupant(Vector2Int cell, out GameObject occupant) => TryGetSlot(occupants, cell, out occupant);

    /// <summary>Claims a cell for an occupant. Fails if out of bounds or already held by a different object.</summary>
    public bool SetOccupant(Vector2Int cell, GameObject go) => SetSlot(occupants, cell, go);

    /// <summary>Clears a cell's occupant only if it still matches the given object.</summary>
    public void ClearOccupant(Vector2Int cell, GameObject go) => ClearSlot(occupants, cell, go);

    public bool TryGetPlacedItem(Vector2Int cell, out GameObject placedItem) => TryGetSlot(placedItems, cell, out placedItem);

    /// <summary>Claims a cell's item slot (ingredient/plate/cup sitting on floor or desk). Independent of occupants.</summary>
    public bool SetPlacedItem(Vector2Int cell, GameObject go) => SetSlot(placedItems, cell, go);

    /// <summary>Clears a cell's placed item only if it still matches the given object.</summary>
    public void ClearPlacedItem(Vector2Int cell, GameObject go) => ClearSlot(placedItems, cell, go);

    private bool TryGetSlot(GameObject[] slots, Vector2Int cell, out GameObject value)
    {
        if (!IsInBounds(cell))
        {
            value = null;
            return false;
        }

        EnsureRuntimeState();
        value = slots[CellIndex(cell)];
        return value != null;
    }

    private bool SetSlot(GameObject[] slots, Vector2Int cell, GameObject go)
    {
        if (!IsInBounds(cell)) return false;

        EnsureRuntimeState();
        int index = CellIndex(cell);
        if (slots[index] != null && slots[index] != go) return false;

        slots[index] = go;
        return true;
    }

    private void ClearSlot(GameObject[] slots, Vector2Int cell, GameObject go)
    {
        if (!IsInBounds(cell)) return;

        EnsureRuntimeState();
        int index = CellIndex(cell);
        if (slots[index] == go) slots[index] = null;
    }

    /// <summary>Cell directly in front of a world position, snapping facing to the nearest 4-way direction.</summary>
    public Vector2Int GetFacingCell(Vector3 worldPosition, Vector2 facing)
    {
        Vector2Int offset = Mathf.Abs(facing.x) > Mathf.Abs(facing.y)
            ? new Vector2Int(facing.x > 0f ? 1 : -1, 0)
            : new Vector2Int(0, facing.y > 0f ? 1 : -1);

        return WorldToCell(worldPosition) + offset;
    }

    /// <summary>
    /// True if an ingredient/plate/cup can be placed on this cell: open floor, or a desk building's
    /// cell (Building.IsDesk) — and no placed item already sitting there. Non-desk buildings (walls,
    /// decorations) occupy their cell but refuse placement on top.
    /// </summary>
    public bool CanPlaceOnCell(Vector2Int cell)
    {
        if (!IsInBounds(cell)) return false;
        if (GetTileType(cell) == TileType.Blocked) return false;
        if (TryGetPlacedItem(cell, out _)) return false;

        if (TryGetOccupant(cell, out GameObject occupant))
        {
            Building building = occupant.GetComponent<Building>();
            return building != null && building.IsDesk;
        }

        return true;
    }

    /// <summary>
    /// Fills buffer with the passable 4-way (up/down/left/right) neighbors of cell. No allocations.
    /// Passability is tile-type only (Blocked tiles, e.g. walls) — occupants (furniture like
    /// tables/chairs) reserve a cell but don't block foot traffic through/around it.
    /// </summary>
    public int GetWalkableNeighborsNonAlloc(Vector2Int cell, Vector2Int[] buffer)
    {
        int count = 0;
        for (int i = 0; i < NeighborOffsets.Length; i++)
        {
            Vector2Int neighbor = cell + NeighborOffsets[i];
            if (GetTileType(neighbor) == TileType.Blocked) continue;

            if (count < buffer.Length) buffer[count] = neighbor;
            count++;
        }
        return count;
    }

    public Vector3 GetPlayerSpawnWorld()
    {
        Vector2Int cell = mapData != null ? mapData.PlayerSpawn : Vector2Int.zero;
        return CellToWorld(cell);
    }

    /// <summary>
    /// Picks a random walkable cell (in bounds, not Blocked, no occupant). Falls back to a linear
    /// scan if random sampling misses, then to (0,0) if the grid has no walkable cell at all.
    /// </summary>
    public Vector2Int GetRandomWalkableCell()
    {
        EnsureRuntimeState();
        int cellCount = Width * Height;

        for (int attempt = 0; attempt < 50; attempt++)
        {
            int index = Random.Range(0, cellCount);
            Vector2Int cell = new Vector2Int(index % Width, index / Width);
            if (IsWalkable(cell)) return cell;
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (IsWalkable(cell)) return cell;
            }
        }

        return Vector2Int.zero;
    }

    public int MonsterSpawnCount => mapData != null ? mapData.MonsterSpawnCount : 0;

    public Vector3 GetMonsterSpawnWorld(int index)
    {
        if (mapData == null || index < 0 || index >= mapData.MonsterSpawnCount) return CellToWorld(Vector2Int.zero);
        return CellToWorld(mapData.GetMonsterSpawn(index).cell);
    }

    public GameObject GetMonsterSpawnPrefab(int index)
    {
        if (mapData == null || index < 0 || index >= mapData.MonsterSpawnCount) return null;
        return mapData.GetMonsterSpawn(index).prefab;
    }

    private int CellIndex(Vector2Int cell) => cell.y * Width + cell.x;

    /// <summary>
    /// Builds/rebuilds the runtime tile & occupant arrays. Cheap to call repeatedly: it only
    /// rebuilds when the effective Width*Height no longer matches the cached arrays, so
    /// edit-mode or early runtime-query calls can call this lazily without ever NRE-ing.
    /// </summary>
    private void EnsureRuntimeState()
    {
        int cellCount = Width * Height;
        if (baseTiles != null && baseTiles.Length == cellCount && occupants != null && occupants.Length == cellCount
            && placedItems != null && placedItems.Length == cellCount && builtFromMap == mapData)
        {
            return;
        }

        baseTiles = new TileType[cellCount];
        occupants = new GameObject[cellCount];
        placedItems = new GameObject[cellCount];
        builtFromMap = mapData;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                baseTiles[CellIndex(cell)] = mapData != null ? mapData.GetTile(cell) : TileType.Floor;
            }
        }
    }

    private void OnValidate()
    {
        if (cellSize < 0.01f) cellSize = 0.01f;
        if (width < 1) width = 1;
        if (height < 1) height = 1;
    }

    private void OnDrawGizmos()
    {
        if (showGrid)
        {
            DrawGridLines();
        }

        if (mapData != null)
        {
            DrawMapDataGizmos();
        }
    }

    private void DrawGridLines()
    {
        Gizmos.color = gridColor;

        Vector3 origin = transform.position;
        float size = CellSize;
        int gridWidth = Width;
        int gridHeight = Height;
        float totalWidth = gridWidth * size;
        float totalHeight = gridHeight * size;

        for (int x = 0; x <= gridWidth; x++)
        {
            float lineX = origin.x + x * size;
            Vector3 start = new Vector3(lineX, origin.y, origin.z);
            Vector3 end = new Vector3(lineX, origin.y + totalHeight, origin.z);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            float lineY = origin.y + y * size;
            Vector3 start = new Vector3(origin.x, lineY, origin.z);
            Vector3 end = new Vector3(origin.x + totalWidth, lineY, origin.z);
            Gizmos.DrawLine(start, end);
        }
    }

    // Reads mapData directly (not the runtime arrays) so it also draws correctly in edit
    // mode, before Awake/EnsureRuntimeState has ever run.
    private void DrawMapDataGizmos()
    {
        float size = CellSize;
        Vector3 cubeSize = Vector3.one * (size * 0.95f);

        Gizmos.color = new Color(1f, 0.25f, 0.25f, 0.35f);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (mapData.GetTile(cell) == TileType.Blocked)
                {
                    Gizmos.DrawCube(CellToWorld(cell), cubeSize);
                }
            }
        }

        float markerRadius = size * 0.2f;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(CellToWorld(mapData.PlayerSpawn), markerRadius);

        Gizmos.color = Color.magenta;
        int monsterCount = mapData.MonsterSpawnCount;
        for (int i = 0; i < monsterCount; i++)
        {
            Gizmos.DrawSphere(CellToWorld(mapData.GetMonsterSpawn(i).cell), markerRadius);
        }
    }
}
