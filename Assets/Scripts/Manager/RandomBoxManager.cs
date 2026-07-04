using System;
using UnityEngine;

public class RandomBoxManager : KSingleton<RandomBoxManager>
{
    // 개봉 성공 시 발행 — UI가 구독해서 팝업 표시 (로직은 UI를 모름)
    public static event Action OnBoxOpened;

    [SerializeField] private int cost = 20;

    public bool TryOpen()
    {
        if (GameManager.Instance.Money < cost) return false;

        GameManager.Instance.AddMoney(-cost);
        OnBoxOpened?.Invoke();
        return true;
    }

<<<<<<< HEAD
    // weight 가중치 롤 — 결과 버프 반환 (팝업이 호출)
    public BuffData Roll()
    {
<<<<<<< HEAD
=======
        UIManager.Instance.ShowPopupUI<RandomBoxPopup>();
    }

=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
    // weight 가중치 롤 — 결과 버프 반환 (팝업이 호출)
    public BuffData Roll()
    {
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
        var buffs = DataTable.Buffs;
        if (buffs == null || buffs.Length == 0) return null;

        int total = 0;
        foreach (var b in buffs) total += b.weight;
        if (total <= 0) return null;

<<<<<<< HEAD
<<<<<<< HEAD
        int roll = UnityEngine.Random.Range(0, total);
=======
        int roll = Random.Range(0, total);
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
        int roll = UnityEngine.Random.Range(0, total);
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
        foreach (var b in buffs)
        {
            roll -= b.weight;
            if (roll < 0) return b;
        }
        return buffs[buffs.Length - 1];
    }
}
