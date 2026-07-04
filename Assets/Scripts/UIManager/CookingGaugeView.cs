using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 조리기구 위 진행바 — CookProgress01 폴링(0→1), 완료 시 자멸
public class CookingGaugeView : MonoBehaviour
{
    [SerializeField] private Image fill;                                  // 채움 이미지 (pivot 왼쪽)
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);

    private CookingStation target;

    public void Bind(CookingStation station)
    {
        target = station;
        transform.position = station.transform.position + worldOffset;

        target.OnCookingFinished += HandleFinished;
    }

    private void OnDestroy()
    {
        if (target != null)
            target.OnCookingFinished -= HandleFinished;
    }

    private void HandleFinished(CookingStation station, IngredientInstance ingredient)
    {
        // 완료 펑 — 흰 플래시 + 펀치 후 소멸
        if (fill != null)
            fill.color = Color.white;

        transform.DOKill();
        var seq = DG.Tweening.DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOScale(transform.localScale * 1.35f, 0.12f));
        seq.Append(transform.DOScale(Vector3.zero, 0.12f));
        seq.OnComplete(() => Destroy(gameObject));

        enabled = false;   // LateUpdate 폴링 중단 (색·스케일 유지)
    }

    private void LateUpdate()
    {
        if (target == null) { Destroy(gameObject); return; }

        if (fill != null)
        {
            var s = fill.rectTransform.localScale;
            s.x = Mathf.Clamp01(target.CookProgress01);
            fill.rectTransform.localScale = s;
        }
    }
}
