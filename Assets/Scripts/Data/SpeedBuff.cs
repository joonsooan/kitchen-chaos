using UnityEngine;

// 바람의 가호 — 이동 속도 증가
[CreateAssetMenu(fileName = "SpeedBuff", menuName = "KitchenChaos/Buff/Speed")]
public class SpeedBuff : BuffData
{
    public float multiplier = 1.5f;

    public override void Apply()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement != null) movement.SpeedMultiplier = multiplier;
    }

    public override void Remove()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement != null) movement.SpeedMultiplier = 1f;
    }
}
