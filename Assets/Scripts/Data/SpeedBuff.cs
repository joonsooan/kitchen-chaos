using UnityEngine;

// 바람의 가호 — 이동 속도 증가
[CreateAssetMenu(fileName = "SpeedBuff", menuName = "KitchenChaos/Buff/Speed")]
public class SpeedBuff : BuffData
{
    public float multiplier = 1.5f;

    public override void Apply()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        // 이속 버프 2종(5%/50%) 동시 활성 가능 — 항상 더 큰 배율 유지
        movement.SpeedMultiplier = Mathf.Max(movement.SpeedMultiplier, multiplier);
    }

    public override void Remove()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        // 다른 이속 버프가 아직 살아 있으면 그 배율로 복귀 (조기 만료가 효과 지우는 것 방지)
        float rest = 1f;
        if (BuffManager.Instance != null)
        {
            foreach (var b in DataTable.Buffs)
                if (b is SpeedBuff s && BuffManager.Instance.IsActive(s))
                    rest = Mathf.Max(rest, s.multiplier);
        }
        movement.SpeedMultiplier = rest;
    }
}
