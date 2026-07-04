using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

// 스프링 장화 점프 — 스페이스바로 바라보는 방향 장애물 뛰어넘기 (포물선 연출).
// JumpBuff가 활성 기간 동안만 enabled 켬.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerController))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpDistance = 2f;      // 최소 점프 거리
    [SerializeField] private float maxJumpDistance = 4f;   // 이 너머까지 막혀있으면 점프 취소
    [SerializeField] private float landingPadding = 0.05f; // 착지 판정 여유 (콜라이더 크기에 더함)
    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpHeight = 0.8f;      // 스프라이트 아크 높이

    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerMovement movement;
    private PlayerController controller;
    private Transform visual;        // 스프라이트 (아크 연출 대상)
    private float visualBaseY;
    private InputAction jumpAction;
    private bool isJumping;

    private void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        col        = GetComponent<Collider2D>();
        movement   = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();

        // 아크 연출용 스프라이트 자식 — 루트 자체가 스프라이트면 아크 생략(스쿼시만)
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null && sprite.transform != transform)
        {
            visual      = sprite.transform;
            visualBaseY = visual.localPosition.y;
        }

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        jumpAction.performed += OnJumpPerformed;
    }

    private void OnEnable()  => jumpAction.Enable();
    private void OnDisable() => jumpAction.Disable();

    private void OnDestroy()
    {
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.Dispose();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isJumping) return;

        // 최소 거리부터 조금씩 늘리며 첫 빈 착지 지점 탐색 (연속 배치 가구도 넘음)
        for (float dist = jumpDistance; dist <= maxJumpDistance; dist += 0.5f)
        {
            Vector2 destination = rb.position + movement.FacingDirection * dist;
            if (IsBlocked(destination)) continue;

            StartJump(destination);
            return;
        }
    }

    // 점프 연출 — 이동 보간 + 스프라이트 포물선 + 스쿼시&스트레치
    private void StartJump(Vector2 destination)
    {
        isJumping = true;
        controller.ChangeState(PlayerState.Busy);   // 이동 입력 잠금
        rb.simulated = false;                       // 점프 중 충돌 무시 (벽 통과)

        float half = jumpDuration * 0.5f;
        var seq = DOTween.Sequence();

        // 수평 이동 (등속)
        seq.Append(transform.DOMove(destination, jumpDuration).SetEase(Ease.Linear));

        // 스프라이트 포물선 (올라갔다 내려옴)
        if (visual != null)
        {
            seq.Join(visual.DOLocalMoveY(visualBaseY + jumpHeight, half).SetEase(Ease.OutQuad));
            seq.Insert(half, visual.DOLocalMoveY(visualBaseY, half).SetEase(Ease.InQuad));
        }

        // 스쿼시&스트레치 — 도약 시 늘어나고 착지에 눌림
        seq.Join(transform.DOScaleY(1.15f, half).SetEase(Ease.OutQuad));
        seq.Insert(half, transform.DOScaleY(0.85f, half * 0.6f).SetEase(Ease.InQuad));
        seq.Append(transform.DOScaleY(1f, 0.1f));

        seq.OnComplete(() =>
        {
            rb.position  = destination;   // 물리 위치 동기
            rb.simulated = true;
            controller.ChangeState(PlayerState.Idle);
            isJumping = false;
        });
    }

    // 트리거(좌석·상호작용 존)와 자기 자신은 무시, 실물 충돌만 차단.
    // 실제 콜라이더 크기로 검사 — 작은 원으로 재면 대각선 착지 시 가구에 몸이 끼어 갇힘
    private bool IsBlocked(Vector2 point)
    {
        Vector2 size   = col != null
            ? (Vector2)col.bounds.size + Vector2.one * landingPadding
            : Vector2.one * 0.6f;
        Vector2 offset = col != null
            ? (Vector2)col.bounds.center - rb.position
            : Vector2.zero;

        foreach (var hit in Physics2D.OverlapBoxAll(point + offset, size, 0f))
        {
            if (hit.isTrigger) continue;
            if (hit.attachedRigidbody == rb) continue;
            return true;
        }
        return false;
    }
}
