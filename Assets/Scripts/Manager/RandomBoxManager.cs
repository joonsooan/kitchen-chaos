using UnityEngine;

public class RandomBoxManager : KSingleton<RandomBoxManager>
{
    [SerializeField] private int cost = 100;

    public bool TryOpen()
    {
        if (GameManager.Instance.Money < cost) return false;

        GameManager.Instance.AddMoney(-cost);
        OnBoxOpened();
        return true;
    }

    private void OnBoxOpened()
    {
        UIManager.Instance.ShowPopupUI<RandomBoxPopup>();
    }

    // weight 가중치 롤 — 결과 버프 반환 (팝업이 호출)
    public BuffData Roll()
    {
        var buffs = DataTable.Buffs;
        if (buffs == null || buffs.Length == 0) return null;

        int total = 0;
        foreach (var b in buffs) total += b.weight;
        if (total <= 0) return null;

        int roll = Random.Range(0, total);
        foreach (var b in buffs)
        {
            roll -= b.weight;
            if (roll < 0) return b;
        }
        return buffs[buffs.Length - 1];
    }
}
