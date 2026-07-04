using DG.Tweening;
using UnityEngine;

public class UIPopup : UIBase
{
    [SerializeField] private float showDuration = 0.3f;

    // ESC로 닫기 허용 — 필수 팝업(튜토리얼·이름입력)은 Init에서 false로
    public bool CloseOnEsc { get; protected set; } = true;
    // 이 팝업이 떠 있는 동안 Time.timeScale을 0으로 고정할지 — UIManager가 open/close 시 관리.
    // 기본 정지 — 예외 팝업만 false로 override
    public virtual bool PauseGameWhileOpen => true;

    public override void Init()
    {
    }

    // 등장 연출 — Panel 뿅 + 딤 페이드인
    private void OnEnable()
    {
        var target = transform.Find("Panel") ?? transform;

        target.localScale = Vector3.zero;
        target.DOScale(1f, showDuration)
              .SetEase(Ease.OutBack)
              .SetUpdate(true)          // 일시정지 중에도 재생
              .SetLink(target.gameObject);

        // 딤(루트 Image) 스르륵
        var dim = GetComponent<UnityEngine.UI.Image>();
        if (dim != null)
        {
            float targetAlpha = dim.color.a;
            var c = dim.color; c.a = 0f; dim.color = c;
            dim.DOFade(targetAlpha, 0.15f).SetUpdate(true).SetLink(gameObject);
        }
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
