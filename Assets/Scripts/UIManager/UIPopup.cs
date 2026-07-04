using DG.Tweening;
using UnityEngine;

public class UIPopup : UIBase
{
    [SerializeField] private float showDuration = 0.3f;

    public override void Init()
    {
    }

    // 등장 연출 — Panel(있으면)만 뿅, 없으면 루트 전체. 딤은 그대로.
    private void OnEnable()
    {
        var target = transform.Find("Panel") ?? transform;

        target.localScale = Vector3.zero;
        target.DOScale(1f, showDuration)
              .SetEase(Ease.OutBack)
              .SetUpdate(true)          // 일시정지 중에도 재생
              .SetLink(target.gameObject);
    }

    // 닫힘 연출 — 역방향 축소 후 콜백에서 실제 파괴 (UIManager가 호출)
    public void PlayHideFx(System.Action onComplete)
    {
        var target = transform.Find("Panel") ?? transform;

        target.DOKill();
        target.DOScale(0f, 0.18f)
              .SetEase(Ease.InBack)
              .SetUpdate(true)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }
}
