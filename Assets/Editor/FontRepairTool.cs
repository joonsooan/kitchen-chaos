using TMPro;
using UnityEditor;
using UnityEngine;

// 제주돌담 SDF 폰트 복구 — 소스 폰트 재연결 + 구운 데이터 완전 리셋 (Dynamic 재베이크)
public static class FontRepairTool
{
    private const string FontAssetPath = "Assets/Fonts/EF_jejudoldam SDF.asset";
    private const string SourceFontPath = "Assets/Fonts/EF_jejudoldam.ttf";

    [MenuItem("Tools/Fix Jejudoldam Font")]
    public static void Fix()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

        if (fontAsset == null || sourceFont == null)
        {
            Debug.LogError($"[FontRepair] 로드 실패 — asset:{fontAsset != null}, ttf:{sourceFont != null}");
            return;
        }

        // 소스 폰트 재연결 (직렬화 필드 직접) + Dynamic 보장
        var so = new SerializedObject(fontAsset);
        so.FindProperty("m_SourceFontFile").objectReferenceValue = sourceFont;
        so.FindProperty("m_AtlasPopulationMode").intValue = (int)AtlasPopulationMode.Dynamic;
        so.ApplyModifiedProperties();

        // 구운 데이터 리셋 — 아틀라스/테이블 전부 비우고 처음부터 다시 굽게
        fontAsset.ClearFontAssetData(true);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FontRepair] 완료 — 텍스트 다시 렌더되는지 확인하세요.");
    }
}
