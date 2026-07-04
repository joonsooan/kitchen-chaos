using DG.Tweening;
using UnityEngine;

// 상시 스쿼시&스트레치 (숨쉬듯 위아래로 눌렸다 늘어남). 상태 무관하게 항상 재생.
[RequireComponent(typeof(CabbageMonster))]
public class CabbageMonsterIdleBobber : MonoBehaviour
{
    [SerializeField] private float squashAmount = 0.06f; // 늘어나는/눌리는 비율
    [SerializeField] private float squashDuration = 1f;

    private Tween squashTween;
    private Vector3 baseScale;

    private void OnEnable() => StartSquash();

    private void OnDisable() => StopSquash();

    private void StartSquash()
    {
        if (squashTween != null && squashTween.IsActive()) return;

        baseScale = transform.localScale;
        Vector3 stretched = new Vector3(baseScale.x * (1f - squashAmount), baseScale.y * (1f + squashAmount), baseScale.z);

        squashTween = transform.DOScale(stretched, squashDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void StopSquash()
    {
        squashTween?.Kill();
        squashTween = null;

        if (baseScale != Vector3.zero) transform.localScale = baseScale;
    }
}
