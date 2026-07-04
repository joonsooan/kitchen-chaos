using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomerGaugeTest))]
public class CustomerGaugeTestEditor : Editor
{
    private const string CustomerPrefabPath = "Assets/Prefabs/Customers/Customer_Rabbit.prefab";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        if (GUILayout.Button("프리팹 자동 할당"))
        {
            AssignPrefabs((CustomerGaugeTest)target);
        }
    }

    // 씬에 GaugeTest 오브젝트 생성 + 프리팹 할당 한 번에
    [MenuItem("Tools/Customer Gauge/Setup Test Scene")]
    private static void SetupTestScene()
    {
        var go   = new GameObject("GaugeTest");
        var test = go.AddComponent<CustomerGaugeTest>();
        AssignPrefabs(test);

        Undo.RegisterCreatedObjectUndo(go, "Setup Gauge Test");
        Selection.activeGameObject = go;
    }

    private static void AssignPrefabs(CustomerGaugeTest test)
    {
        var customerGo = AssetDatabase.LoadAssetAtPath<GameObject>(CustomerPrefabPath);
        if (customerGo == null)
        {
            Debug.LogWarning($"[GaugeTest] 못 찾음: {CustomerPrefabPath}");
            return;
        }

        var so = new SerializedObject(test);
        so.FindProperty("customerPrefab").objectReferenceValue = customerGo.GetComponent<Customer>();
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(test);
    }
}
