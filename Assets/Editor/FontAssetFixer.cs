using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// MemomentKkukkukk SDF 복구 — Inter로 구워진 글리프 테이블이 남아 있어
// 글리프 인덱스가 어긋나 일부 한글이 엉뚱한 글자로 렌더링되는 문제.
// 테이블·아틀라스를 비우고 FaceInfo를 실제 ttf 기준으로 재생성한다 (동적 모드라 글리프는 사용 시 재생성).
public static class FontAssetFixer
{
    private const string AssetPath = "Assets/Fonts/MemomentKkukkukk SDF.asset";
    private const string TtfPath   = "Assets/Fonts/MemomentKkukkukk.ttf";

    [MenuItem("Tools/Fonts/Fix MemomentKkukkukk SDF")]
    public static void Fix()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        var ttf       = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (fontAsset == null || ttf == null)
        {
            Debug.LogError($"[FontAssetFixer] 에셋 로드 실패 — {AssetPath} / {TtfPath}");
            return;
        }

        int pointSize = (int)fontAsset.faceInfo.pointSize;

        // 1) 잘못 구워진 글리프/문자 테이블 + 아틀라스 초기화
        fontAsset.ClearFontAssetData(setAtlasSizeToZero: true);

        // 2) FaceInfo를 실제 소스 폰트 기준으로 재생성 (현재 Inter 메트릭이 박혀 있음)
        if (FontEngine.InitializeFontEngine() == FontEngineError.Success &&
            FontEngine.LoadFontFace(ttf, pointSize) == FontEngineError.Success)
        {
            var face = FontEngine.GetFaceInfo();
            typeof(TMP_FontAsset)
                .GetField("m_FaceInfo", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(fontAsset, face);
        }
        else
        {
            Debug.LogWarning("[FontAssetFixer] FontEngine 로드 실패 — FaceInfo 갱신 생략 (글리프는 정상 재생성됨)");
        }

        // 3) 아틀라스 가득 참 방지 + 빌드 시 동적 데이터 자동 초기화
        var so = new SerializedObject(fontAsset);
        so.FindProperty("m_IsMultiAtlasTexturesEnabled").boolValue = true;
        so.FindProperty("m_ClearDynamicDataOnBuild").boolValue     = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        Debug.Log($"[FontAssetFixer] 완료 — FaceInfo: {fontAsset.faceInfo.familyName}, 테이블 초기화됨. 플레이 시 글리프 재생성.");
    }
}
