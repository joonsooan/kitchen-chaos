using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

// 제주돌담 폰트를 TMP Font Asset(Dynamic)으로 만들고 프로젝트 전체 TMP 텍스트에 일괄 적용
public static class ApplyKoreanFont
{
    private const string FontPath      = "Assets/Fonts/EF_jejudoldam.ttf";
    private const string FontAssetPath = "Assets/Fonts/EF_jejudoldam SDF.asset";

    [MenuItem("Tools/Font/Apply JejuDoldam To All TMP")]
    public static void Apply()
    {
        var fontAsset = GetOrCreateFontAsset();
        if (fontAsset == null) return;

        int changed = 0;

        // 1) 모든 프리팹의 TMP 교체
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            bool dirty = false;

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.font == fontAsset) continue;
                text.font = fontAsset;
                dirty = true;
                changed++;
            }

            if (dirty) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        // 2) 열려있는 씬의 TMP 교체
        foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text.font == fontAsset) continue;
            text.font = fontAsset;
            EditorUtility.SetDirty(text);
            changed++;
        }

        // 3) TMP 기본 폰트 교체 — 이후 새 텍스트도 자동 적용
        var settings = TMP_Settings.instance;
        if (settings != null)
        {
            var so = new SerializedObject(settings);
            so.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ApplyKoreanFont] 완료 — TMP {changed}개 교체 + 기본 폰트 설정");
    }

    private static TMP_FontAsset GetOrCreateFontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null) return existing;

        var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (font == null)
        {
            Debug.LogError($"[ApplyKoreanFont] 폰트 못 찾음: {FontPath}");
            return null;
        }

        // Dynamic — 한글 글리프를 런타임에 필요할 때 아틀라스에 굽는다 (사전 베이크 불필요)
        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font, 60, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
            1024, 1024, AtlasPopulationMode.Dynamic, true);

        fontAsset.name = Path.GetFileNameWithoutExtension(FontAssetPath);
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // 머티리얼·아틀라스 서브에셋으로 저장
        fontAsset.material.name = fontAsset.name + " Material";
        AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        foreach (var tex in fontAsset.atlasTextures)
        {
            tex.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(tex, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(FontAssetPath);
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
    }
}
