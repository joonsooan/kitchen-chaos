using UnityEngine;

// 꽝 — 아무것도 들어있지 않았다...
[CreateAssetMenu(fileName = "EmptyBuff", menuName = "KitchenChaos/Buff/Empty")]
public class EmptyBuff : BuffData
{
    public override void Apply() { }
    public override void Remove() { }
}
