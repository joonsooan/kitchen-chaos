using UnityEngine;

// 파워 업 — 기간 동안 양상추 몬스터·잡초를 한 방에 처치.
// CabbageMonster/WeedTile의 Hit이 OneShotActive를 참조 (SpeedMultiplier 훅과 동일 패턴).
[CreateAssetMenu(fileName = "PowerBuff", menuName = "KitchenChaos/Buff/Power")]
public class PowerBuff : BuffData
{
    public static bool OneShotActive { get; private set; }

    public override void Apply()  => OneShotActive = true;
    public override void Remove() => OneShotActive = false;
}
