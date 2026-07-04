using UnityEngine;

<<<<<<< HEAD
<<<<<<< HEAD
// 스프링 장화 — 스페이스바로 장애물 뛰어넘기 (기간 동안 PlayerJump 활성)
=======
// 스프링 장화 — 주방 건물 뛰어넘기
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
// 스프링 장화 — 스페이스바로 장애물 뛰어넘기 (기간 동안 PlayerJump 활성)
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
[CreateAssetMenu(fileName = "JumpBuff", menuName = "KitchenChaos/Buff/Jump")]
public class JumpBuff : BuffData
{
    public override void Apply()
    {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        var jump = movement.GetComponent<PlayerJump>();
        if (jump == null) jump = movement.gameObject.AddComponent<PlayerJump>();
        jump.enabled = true;
<<<<<<< HEAD
=======
        // TODO(2단계): 통행/충돌 로직 훅 연결
        Debug.Log("[JumpBuff] 건물 뛰어넘기 활성");
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
    }

    public override void Remove()
    {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
        var movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement == null) return;

        var jump = movement.GetComponent<PlayerJump>();
        if (jump != null) jump.enabled = false;
<<<<<<< HEAD
=======
        Debug.Log("[JumpBuff] 건물 뛰어넘기 해제");
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
    }
}
