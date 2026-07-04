using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// 랜덤박스 롤 분포 테스트 — RandomBoxManager.Roll과 동일한 가중치 로직으로 N회 시뮬레이션.
// Tools 메뉴에서 실행, 결과는 Console에 표 형태로 출력 (기대 확률 ±2%p 허용).
public static class RandomBoxRollTest
{
    private const int Rolls = 100000;
    private const float TolerancePct = 2f;

    [MenuItem("Tools/Test Random Box Roll")]
    public static void Run()
    {
        var buffs = Resources.LoadAll<BuffData>("Buff Data");
        if (buffs == null || buffs.Length == 0)
        {
            Debug.LogError("[RollTest] Buff Data 에셋 없음");
            return;
        }

        int total = 0;
        foreach (var b in buffs) total += b.weight;

        var counts = new Dictionary<BuffData, int>();
        foreach (var b in buffs) counts[b] = 0;

        // RandomBoxManager.Roll과 동일한 로직
        for (int i = 0; i < Rolls; i++)
        {
            int roll = Random.Range(0, total);
            foreach (var b in buffs)
            {
                roll -= b.weight;
                if (roll < 0) { counts[b]++; break; }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[RollTest] {Rolls:N0}회 롤 (가중치 합 {total})");
        bool allPass = true;
        foreach (var b in buffs)
        {
            float expected = 100f * b.weight / total;
            float actual   = 100f * counts[b] / Rolls;
            bool pass = Mathf.Abs(expected - actual) <= TolerancePct;
            allPass &= pass;
            sb.AppendLine($"  {(pass ? "PASS" : "FAIL")}  {b.buffName,-10} 기대 {expected:F1}%  실제 {actual:F1}%  ({counts[b]:N0}회)");
        }
        sb.AppendLine(allPass ? "[RollTest] 전체 PASS" : "[RollTest] FAIL 있음 — weight 확인 필요");

        if (allPass) Debug.Log(sb.ToString());
        else Debug.LogError(sb.ToString());
    }
}
