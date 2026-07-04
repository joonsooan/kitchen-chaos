using DG.Tweening;
using UnityEngine;

// Idle / Busy(작업 중) 상태일 때 천천히 스쿼시&스트레치 (숨쉬듯 위아래로 눌렸다 늘어남).
// PlayerJump가 Busy 상태에서 자체 포물선 연출을 트는 동안엔 TweenId로 강제 Kill 해서 충돌 방지.
[RequireComponent(typeof(PlayerController))]
public class PlayerIdleBobber : MonoBehaviour
{
    public const string TweenId = "PlayerIdleBob";

    [SerializeField] private float squashAmount = 0.06f; // 늘어나는/눌리는 비율
    [SerializeField] private float squashDuration = 1f;

    private Tween squashTween;
    private Vector3 baseScale;

    private void OnEnable() => PlayerController.OnStateChanged += HandleStateChanged;

    private void OnDisable()
    {
        PlayerController.OnStateChanged -= HandleStateChanged;
        StopSquash();
    }

    private void HandleStateChanged(PlayerState state)
    {
        if (state == PlayerState.Idle || state == PlayerState.Busy) StartSquash();
        else StopSquash();
    }

    private void StartSquash()
    {
        if (squashTween != null && squashTween.IsActive()) return;

        baseScale = transform.localScale;
        Vector3 stretched = new Vector3(baseScale.x * (1f - squashAmount), baseScale.y * (1f + squashAmount), baseScale.z);

        squashTween = transform.DOScale(stretched, squashDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId(TweenId)
            .SetLink(gameObject);
    }

    private void StopSquash()
    {
        squashTween?.Kill();
        squashTween = null;

        if (baseScale != Vector3.zero) transform.localScale = baseScale;
    }
}
