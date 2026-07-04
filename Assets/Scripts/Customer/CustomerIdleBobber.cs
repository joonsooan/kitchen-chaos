using DG.Tweening;
using UnityEngine;

// 좌석에서 대기(Waiting) 중일 때만 천천히 스쿼시&스트레치 (숨쉬듯 위아래로 눌렸다 늘어남)
[RequireComponent(typeof(Customer))]
public class CustomerIdleBobber : MonoBehaviour
{
    [SerializeField] private float squashAmount = 0.06f; // 늘어나는/눌리는 비율
    [SerializeField] private float squashDuration = 1f;

    private Customer customer;
    private Tween squashTween;
    private Vector3 baseScale;

    private void Awake()
    {
        customer = GetComponent<Customer>();
    }

    private void OnEnable() => customer.OnStateChanged += HandleStateChanged;

    private void OnDisable()
    {
        customer.OnStateChanged -= HandleStateChanged;
        StopSquash();
    }

    private void HandleStateChanged(CustomerState state)
    {
        if (state == CustomerState.Waiting) StartSquash();
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
            .SetLink(gameObject);
    }

    private void StopSquash()
    {
        squashTween?.Kill();
        squashTween = null;

        if (baseScale != Vector3.zero) transform.localScale = baseScale;
    }
}
