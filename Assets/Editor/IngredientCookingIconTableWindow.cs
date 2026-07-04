using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 재료(행) x 조리법(열) 그리드로 cookingMethodIcons 편집하는 툴
public class IngredientCookingIconTableWindow : EditorWindow
{
    private Vector2 _scroll;
    private Ingredient[] _ingredients = Array.Empty<Ingredient>();
    private CookingMethod[] _methods = Array.Empty<CookingMethod>();

    private const float NameColumnWidth = 150f;
    private const float CellWidth = 64f;
    private const float CellHeight = 64f;

    [MenuItem("KitchenChaos/Ingredient Cooking Icon Table")]
    public static void Open()
    {
        var window = GetWindow<IngredientCookingIconTableWindow>("Cooking Icon Table");
        window.minSize = new Vector2(400f, 300f);
        window.RefreshIngredients();
    }

    private void OnEnable() => RefreshIngredients();

    private void RefreshIngredients()
    {
        _methods = ((CookingMethod[])Enum.GetValues(typeof(CookingMethod)))
            .Where(m => m != CookingMethod.None)
            .ToArray();

        _ingredients = AssetDatabase.FindAssets("t:Ingredient")
            .Select(guid => AssetDatabase.LoadAssetAtPath<Ingredient>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(ingredient => ingredient != null)
            .OrderBy(ingredient => ingredient.ingredientName)
            .ToArray();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
            RefreshIngredients();

        if (_ingredients.Length == 0)
        {
            EditorGUILayout.HelpBox("No Ingredient assets found.", MessageType.Info);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        // 헤더 행
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Ingredient", EditorStyles.boldLabel, GUILayout.Width(NameColumnWidth));
        foreach (var method in _methods)
            GUILayout.Label(method.ToString(), EditorStyles.boldLabel, GUILayout.Width(CellWidth));
        EditorGUILayout.EndHorizontal();

        foreach (var ingredient in _ingredients)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(ingredient.ingredientName, GUILayout.Width(NameColumnWidth));

            EnsureArraySize(ingredient);

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < _methods.Length; i++)
            {
                int index = (int)_methods[i];
                ingredient.cookingMethodIcons[index] = (Sprite)EditorGUILayout.ObjectField(
                    ingredient.cookingMethodIcons[index], typeof(Sprite), false,
                    GUILayout.Width(CellWidth), GUILayout.Height(CellHeight));
            }
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(ingredient);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private static void EnsureArraySize(Ingredient ingredient)
    {
        int required = Enum.GetValues(typeof(CookingMethod)).Length;
        if (ingredient.cookingMethodIcons != null && ingredient.cookingMethodIcons.Length == required)
            return;

        var resized = new Sprite[required];
        if (ingredient.cookingMethodIcons != null)
            Array.Copy(ingredient.cookingMethodIcons, resized,
                Math.Min(ingredient.cookingMethodIcons.Length, required));
        ingredient.cookingMethodIcons = resized;
        EditorUtility.SetDirty(ingredient);
    }
}
