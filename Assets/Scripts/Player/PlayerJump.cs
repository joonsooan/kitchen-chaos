using UnityEngine;
using UnityEngine.InputSystem;

// 스프링 장화 점프 — 스페이스바로 바라보는 방향 장애물 뛰어넘기.
// JumpBuff가 활성 기간 동안만 enabled 켬.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpDistance = 2f;      // 최소 점프 거리
    [SerializeField] private float maxJumpDistance = 4f;   // 이 너머까지 막혀있으면 점프 취소
    [SerializeField] private float landingRadius = 0.3f;   // 착지 지점 여유 반경

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private InputAction jumpAction;

    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        jumpAction.performed += OnJumpPerformed;
    }

    private void OnEnable()
    {
        jumpAction.Enable();
        Debug.Log("[PlayerJump] 점프 활성 — 스페이스바");
    }
    private void OnDisable() => jumpAction.Disable();

    private void OnDestroy()
    {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.Dispose();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // 최소 거리부터 조금씩 늘리며 첫 빈 착지 지점 탐색 (연속 배치 가구도 넘음)
        for (float dist = jumpDistance; dist <= maxJumpDistance; dist += 0.5f)
        {
            Vector2 destination = rb.position + movement.FacingDirection * dist;
            if (IsBlocked(destination)) continue;

            rb.position = destination;
            return;
        }

        Debug.Log($"[PlayerJump] {maxJumpDistance}유닛까지 전부 막힘 — 점프 취소");
    }

    // 트리거(좌석·상호작용 존)와 자기 자신은 무시, 실물 충돌만 차단
    private bool IsBlocked(Vector2 point)
    {
        foreach (var hit in Physics2D.OverlapCircleAll(point, landingRadius))
        {
            if (hit.isTrigger) continue;
            if (hit.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }
}
