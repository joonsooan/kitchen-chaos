using UnityEngine;

// 스프링 장화 — 주방 건물 뛰어넘기
[CreateAssetMenu(fileName = "JumpBuff", menuName = "KitchenChaos/Buff/Jump")]
public class JumpBuff : BuffData
{
    public override void Apply()
    {
        // TODO(2단계): 통행/충돌 로직 훅 연결
        Debug.Log("[JumpBuff] 건물 뛰어넘기 활성");
    }

    public override void Remove()
    {
        Debug.Log("[JumpBuff] 건물 뛰어넘기 해제");
    }
}
