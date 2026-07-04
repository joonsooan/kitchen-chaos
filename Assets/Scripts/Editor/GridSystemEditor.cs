using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridSystem))]
public class GridSystemEditor : UnityEditor.Editor
{
    private SerializedProperty mapDataProp;
    private SerializedProperty showGridProp;
    private SerializedProperty gridColorProp;
    private SerializedProperty cellSizeProp;
    private SerializedProperty widthProp;
    private SerializedProperty heightProp;

    private void OnEnable()
    {
        mapDataProp = serializedObject.FindProperty("mapData");
        showGridProp = serializedObject.FindProperty("showGrid");
        gridColorProp = serializedObject.FindProperty("gridColor");
        cellSizeProp = serializedObject.FindProperty("cellSize");
        widthProp = serializedObject.FindProperty("width");
        heightProp = serializedObject.FindProperty("height");
    }

    public override void OnInspectorGUI()
    {
        GridSystem gridSystem = (GridSystem)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(mapDataProp);
        EditorGUILayout.PropertyField(showGridProp);
        EditorGUILayout.PropertyField(gridColorProp);

        if (gridSystem.Map != null)
        {
            EditorGUILayout.HelpBox(
                $"크기·셀 값은 MapData 에셋에서 편집합니다 — 현재 {gridSystem.Width}×{gridSystem.Height}, cellSize {gridSystem.CellSize}",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("MapData 미연결 — 아래 예비 값 사용 중", MessageType.Warning);

            EditorGUILayout.PropertyField(cellSizeProp);
            EditorGUILayout.PropertyField(widthProp);
            EditorGUILayout.PropertyField(heightProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
