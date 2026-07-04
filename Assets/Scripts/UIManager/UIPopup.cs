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
}
