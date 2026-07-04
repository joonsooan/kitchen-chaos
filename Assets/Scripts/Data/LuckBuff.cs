using UnityEngine;

// 행운의 네잎클로버 — 점수·코인 획득 2배
[CreateAssetMenu(fileName = "LuckBuff", menuName = "KitchenChaos/Buff/Luck")]
public class LuckBuff : BuffData
{
    public int rewardMultiplier = 2;

    public override void Apply()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RewardMultiplier = rewardMultiplier;
    }

    public override void Remove()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RewardMultiplier = 1;
    }
}
