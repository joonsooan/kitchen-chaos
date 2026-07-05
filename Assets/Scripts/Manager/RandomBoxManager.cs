using System;
using UnityEngine;

public class RandomBoxManager : KSingleton<RandomBoxManager>
{
    // 개봉 성공 시 발행 — UI가 구독해서 팝업 표시 (로직은 UI를 모름)
    public static event Action OnBoxOpened;

    [SerializeField] private int cost = 20;

    public int Cost => cost;

    public bool TryOpen()
    {
        if (GameManager.Instance.Money < cost) return false;

        GameManager.Instance.AddMoney(-cost);
        OnBoxOpened?.Invoke();
        return true;
    }

    // 결과창 "다시 돌리기" — 코인만 차감, OnBoxOpened 미발행 (팝업 이미 열림)
    public bool TryPayReroll()
    {
        if (GameManager.Instance == null || GameManager.Instance.Money < cost) return false;

        GameManager.Instance.AddMoney(-cost);
        return true;
    }

    // weight 가중치 롤 — 결과 버프 반환 (팝업이 호출)
    public BuffData Roll()
    {
        var buffs = DataTable.Buffs;
        if (buffs == null || buffs.Length == 0) return null;

        int total = 0;
        foreach (var b in buffs) total += b.weight;
        if (total <= 0) return null;

        int roll = UnityEngine.Random.Range(0, total);
        foreach (var b in buffs)
        {
            roll -= b.weight;
            if (roll < 0) return b;
        }
        return buffs[buffs.Length - 1];
    }
}
