using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    private enum Brush
    {
        Floor,
        Blocked,
        Player,
        Monster
    }

    private static readonly string[] BrushLabels = { "Floor", "Blocked", "Player", "Monster" };
    private static readonly Vector2Int NoCell = new Vector2Int(int.MinValue, int.MinValue);

    private const float MinCellPixels = 14f;
    private const float MaxCellPixels = 28f;
    private const float CellGap = 1f;
    private const float MarkerInset = 3f;
    private const float InspectorMargin = 40f;

    private SerializedProperty widthProp;
    private SerializedProperty heightProp;
    private SerializedProperty cellSizeProp;
    private SerializedProperty defaultTileProp;
    private SerializedProperty tileOverridesProp;
    private SerializedProperty playerSpawnProp;
    private SerializedProperty monsterSpawnsProp;

    private Brush brush = Brush.Floor;
    private bool showRawData;
    private Vector2Int lastPaintedCell = NoCell;

    private void OnEnable()
    {
        widthProp = serializedObject.FindProperty("width");
        heightProp = serializedObject.FindProperty("height");
        cellSizeProp = serializedObject.FindProperty("cellSize");
        defaultTileProp = serializedObject.FindProperty("defaultTile");
        tileOverridesProp = serializedObject.FindProperty("tileOverrides");
        playerSpawnProp = serializedObject.FindProperty("playerSpawn");
        monsterSpawnsProp = serializedObject.FindProperty("monsterSpawns");
    }

    public override void OnInspectorGUI()
    {
        MapData map = (MapData)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        EditorGUILayout.PropertyField(cellSizeProp);
        EditorGUILayout.PropertyField(defaultTileProp);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
        brush = (Brush)GUILayout.Toolbar((int)brush, BrushLabels);

        EditorGUILayout.Space();
        DrawMapGrid(map);

        EditorGUILayout.Space();
        showRawData = EditorGUILayout.Foldout(showRawData, "Raw Data", true);
        if (showRawData)
        {
            // Re-sync: brush painting above mutates the object directly (not through
            // SerializedProperty), so the cached serializedObject snapshot must be
            // refreshed before we display/edit it here - otherwise ApplyModifiedProperties
            // could stomp a paint that just happened this same GUI pass.
            serializedObject.Update();
            EditorGUILayout.PropertyField(tileOverridesProp, true);
            EditorGUILayout.PropertyField(playerSpawnProp, true);
            EditorGUILayout.PropertyField(monsterSpawnsProp, true);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawMapGrid(MapData map)
    {
        int width = map.Width;
        int height = map.Height;
        if (width <= 0 || height <= 0) return;

        float availableWidth = EditorGUIUtility.currentViewWidth - InspectorMargin;
        float cellPixels = Mathf.Clamp(availableWidth / width, MinCellPixels, MaxCellPixels);

        float totalWidth = width * cellPixels;
        float totalHeight = height * cellPixels;

        // Reserve the layout rect unconditionally (every event type) so Layout/Repaint stay
        // in sync; only branch on event type for drawing vs. input afterwards.
        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        Event evt = Event.current;

        if (evt.type == EventType.Repaint)
        {
            DrawCells(map, gridRect, width, height, cellPixels);
        }
        else if (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)
        {
            HandlePaintInput(map, gridRect, evt, height, cellPixels);
        }
        else if (evt.type == EventType.MouseUp)
        {
            lastPaintedCell = NoCell;
        }
    }

    private void DrawCells(MapData map, Rect gridRect, int width, int height, float cellPixels)
    {
        Vector2Int playerSpawn = map.PlayerSpawn;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Rect cellRect = GetCellRect(gridRect, cell, height, cellPixels);

                TileType tile = map.GetTile(cell);
                Color tileColor = tile == TileType.Blocked
                    ? new Color(0.8f, 0.25f, 0.25f)
                    : new Color(0.35f, 0.35f, 0.35f);
                EditorGUI.DrawRect(cellRect, tileColor);

                if (cell == playerSpawn)
                {
                    EditorGUI.DrawRect(Inset(cellRect, MarkerInset), Color.green);
                }

                if (IsMonsterSpawn(map, cell))
                {
                    EditorGUI.DrawRect(Inset(cellRect, MarkerInset), Color.magenta);
                }
            }
        }
    }

    private void HandlePaintInput(MapData map, Rect gridRect, Event evt, int height, float cellPixels)
    {
        if (!gridRect.Contains(evt.mousePosition)) return;

        Vector2Int cell = GetCellAtPosition(gridRect, evt.mousePosition, height, cellPixels);
        if (!map.IsInBounds(cell)) return;

        bool isDrag = evt.type == EventType.MouseDrag;
        bool isMonsterDrag = brush == Brush.Monster && isDrag; // toggle only fires on a discrete click
        bool alreadyPainted = cell == lastPaintedCell;

        if (!isMonsterDrag && !alreadyPainted)
        {
            PaintCell(map, cell);
            lastPaintedCell = cell;
        }

        evt.Use();
        Repaint();
    }

    private void PaintCell(MapData map, Vector2Int cell)
    {
        Undo.RecordObject(map, "Paint Map");

        switch (brush)
        {
            case Brush.Floor:
                map.SetTile(cell, TileType.Floor);
                break;
            case Brush.Blocked:
                map.SetTile(cell, TileType.Blocked);
                break;
            case Brush.Player:
                map.SetPlayerSpawn(cell);
                break;
            case Brush.Monster:
                map.ToggleMonsterSpawn(cell);
                break;
        }

        EditorUtility.SetDirty(map);
        SceneView.RepaintAll(); // keep GridSystem's mapData gizmos live while painting
    }

    private static bool IsMonsterSpawn(MapData map, Vector2Int cell)
    {
        int count = map.MonsterSpawnCount;
        for (int i = 0; i < count; i++)
        {
            if (map.GetMonsterSpawn(i).cell == cell) return true;
        }
        return false;
    }

    // Rows are drawn top-to-bottom with row 0 = cell y = height-1, so the inspector grid
    // visually matches the scene's Y-up orientation (cell y=0 at the bottom).
    private static Rect GetCellRect(Rect gridRect, Vector2Int cell, int height, float cellPixels)
    {
        int row = height - 1 - cell.y;
        float x = gridRect.x + cell.x * cellPixels;
        float y = gridRect.y + row * cellPixels;
        return new Rect(x, y, cellPixels - CellGap, cellPixels - CellGap);
    }

    private static Vector2Int GetCellAtPosition(Rect gridRect, Vector2 mousePosition, int height, float cellPixels)
    {
        int col = Mathf.FloorToInt((mousePosition.x - gridRect.x) / cellPixels);
        int row = Mathf.FloorToInt((mousePosition.y - gridRect.y) / cellPixels);
        int cellY = height - 1 - row;
        return new Vector2Int(col, cellY);
    }

    private static Rect Inset(Rect rect, float amount)
    {
        return new Rect(rect.x + amount, rect.y + amount,
            Mathf.Max(0f, rect.width - amount * 2f), Mathf.Max(0f, rect.height - amount * 2f));
    }
}
