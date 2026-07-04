using TMPro;
using UnityEditor;
using UnityEngine;

// SDF 폰트 에셋 유지보수 도구.
// 프로젝트 전체 TMP가 참조하는 SDF 에셋(guid 고정)의 소스 폰트를 갈아끼우고
// 구운 데이터를 리셋 → 모든 텍스트가 새 폰트로 다시 구워짐.
public static class FontRepairTool
{
    private const string FontAssetPath  = "Assets/Fonts/EF_jejudoldam SDF.asset";
    private const string SourceFontPath = "Assets/Fonts/MemomentKkukkukk.ttf";

    [MenuItem("Tools/Apply Kkukkukk Font")]
    public static void Fix()
    {
        var fontAsset  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

        if (fontAsset == null || sourceFont == null)
        {
            Debug.LogError($"[FontRepair] 로드 실패 — asset:{fontAsset != null}, ttf:{sourceFont != null}");
            return;
        }

        // 소스 폰트 교체 (직렬화 필드 직접) + Dynamic 보장
        var so = new SerializedObject(fontAsset);
        so.FindProperty("m_SourceFontFile").objectReferenceValue = sourceFont;
        so.FindProperty("m_SourceFontFileGUID").stringValue =
            AssetDatabase.AssetPathToGUID(SourceFontPath);
        so.FindProperty("m_AtlasPopulationMode").intValue = (int)AtlasPopulationMode.Dynamic;
        so.ApplyModifiedProperties();

        // 구운 데이터 리셋
        fontAsset.ClearFontAssetData(true);

        // FaceInfo(줄간격 등 메트릭) 새 폰트 기준으로 갱신 — FontEngine 직접 사용
        if (UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(sourceFont, 60)
            == UnityEngine.TextCore.LowLevel.FontEngineError.Success)
        {
            var face = UnityEngine.TextCore.LowLevel.FontEngine.GetFaceInfo();
            var so2 = new SerializedObject(fontAsset);
            var fi = so2.FindProperty("m_FaceInfo");
            fi.FindPropertyRelative("m_FamilyName").stringValue = face.familyName;
            fi.FindPropertyRelative("m_StyleName").stringValue = face.styleName;
            fi.FindPropertyRelative("m_PointSize").floatValue = face.pointSize;
            fi.FindPropertyRelative("m_Scale").floatValue = face.scale;
            fi.FindPropertyRelative("m_LineHeight").floatValue = face.lineHeight;
            fi.FindPropertyRelative("m_AscentLine").floatValue = face.ascentLine;
            fi.FindPropertyRelative("m_CapLine").floatValue = face.capLine;
            fi.FindPropertyRelative("m_MeanLine").floatValue = face.meanLine;
            fi.FindPropertyRelative("m_Baseline").floatValue = face.baseline;
            fi.FindPropertyRelative("m_DescentLine").floatValue = face.descentLine;
            fi.FindPropertyRelative("m_SuperscriptOffset").floatValue = face.superscriptOffset;
            fi.FindPropertyRelative("m_SuperscriptSize").floatValue = face.superscriptSize;
            fi.FindPropertyRelative("m_SubscriptOffset").floatValue = face.subscriptOffset;
            fi.FindPropertyRelative("m_SubscriptSize").floatValue = face.subscriptSize;
            fi.FindPropertyRelative("m_UnderlineOffset").floatValue = face.underlineOffset;
            fi.FindPropertyRelative("m_UnderlineThickness").floatValue = face.underlineThickness;
            fi.FindPropertyRelative("m_StrikethroughOffset").floatValue = face.strikethroughOffset;
            fi.FindPropertyRelative("m_StrikethroughThickness").floatValue = face.strikethroughThickness;
            fi.FindPropertyRelative("m_TabWidth").floatValue = face.tabWidth;
            so2.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[FontRepair] FaceInfo 갱신 실패 — 메트릭은 이전 폰트 기준 유지");
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[FontRepair] 완료 — 모든 TMP 텍스트가 Kkukkukk 폰트로 다시 구워집니다.");
    }
}
