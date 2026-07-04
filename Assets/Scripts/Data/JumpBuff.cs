using UnityEngine;

// 스프링 장화 — 스페이스바로 장애물 뛰어넘기 (기간 동안 PlayerJump 활성)
[CreateAssetMenu(fileName = "JumpBuff", menuName = "KitchenChaos/Buff/Jump")]
public class JumpBuff : BuffData
{
    public override void Apply()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        var jump = movement.GetComponent<PlayerJump>();
        if (jump == null) jump = movement.gameObject.AddComponent<PlayerJump>();
        jump.enabled = true;
    }

    public override void Remove()
    {
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        var jump = movement.GetComponent<PlayerJump>();
        if (jump != null) jump.enabled = false;
    }
}
