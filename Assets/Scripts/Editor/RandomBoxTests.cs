using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 랜덤박스 Roll 검증 — Tools/Tests/Run RandomBox Tests 원클릭.
// (Test Runner는 게임 코드 asmdef화가 필요해서 잼에선 에디터 검증으로 대체)
public static class RandomBoxTests
{
    private const int RollCount = 100_000;
    private const float TolerancePct = 1.5f;   // 확률 허용 오차 (%p)

    [MenuItem("Tools/Tests/Run RandomBox Tests")]
    public static void RunAll()
    {
        int passed = 0, failed = 0;

        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passed++; Debug.Log($"✅ {name} {detail}"); }
            else    { failed++; Debug.LogError($"❌ {name} {detail}"); }
        }

        // ── 준비: 임시 RandomBoxManager (씬 오브젝트) ─────────────
        var go = new GameObject("~RandomBoxTest");
        var box = go.AddComponent<RandomBoxManager>();

        try
        {
            var buffs = DataTable.Buffs;
            
            // 1) 풀 로드 확인
            Check("버프 풀 로드 (4종)", buffs != null && buffs.Length == 4,
                  $"— {buffs?.Length ?? 0}개");

            // 2) weight 합 100 (기획 40/25/25/10)
            int totalWeight = buffs.Sum(b => b.weight);
            Check("weight 합 100", totalWeight == 100, $"— 합 {totalWeight}");

            // 3) Roll 분포 — 10만 번 롤, 각 버프 실측 확률이 weight ±1.5%p 이내
            var counts = new Dictionary<BuffData, int>();
            foreach (var b in buffs) counts[b] = 0;

            for (int i = 0; i < RollCount; i++)
            {
                var r = box.Roll();
                if (r == null) { Check("Roll null 없음", false); break; }
                counts[r]++;
            }

            foreach (var b in buffs)
            {
                float actualPct = counts[b] * 100f / RollCount;
                bool ok = Mathf.Abs(actualPct - b.weight) <= TolerancePct;
                Check($"분포 {b.buffName}", ok,
                      $"— 기대 {b.weight}% 실측 {actualPct:F2}%");
            }

            // 4) 엣지: 매 롤이 풀 안의 버프만 반환
            bool allInPool = true;
            for (int i = 0; i < 1000; i++)
                if (!buffs.Contains(box.Roll())) { allInPool = false; break; }
            Check("Roll 결과 항상 풀 내부", allInPool);

            // 5) 엣지: duration 0(꽝)은 BuffManager.Activate가 무시해야
            var empty = buffs.FirstOrDefault(b => b.duration <= 0f);
            Check("꽝 버프 존재 (duration 0)", empty != null,
                  empty != null ? $"— {empty.buffName}" : "");
        }
        finally
        {
            Object.DestroyImmediate(go);
        }

        Debug.Log($"[RandomBoxTests] 완료 — 통과 {passed} / 실패 {failed}");
    }
}
