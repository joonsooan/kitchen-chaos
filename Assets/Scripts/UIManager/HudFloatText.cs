using DG.Tweening;
using TMPro;
using UnityEngine;

// HUD 위 플로팅 텍스트 — 위로 떠오르며 페이드아웃 후 파괴 (코인 +N 등)
public class HudFloatText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float rise     = 60f;    // UI 픽셀
    [SerializeField] private float duration = 0.8f;

    public void Show(string text, Color color)
    {
        label.text  = text;
        label.color = color;

        var rt  = (RectTransform)transform;
        var seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + rise, duration).SetEase(Ease.OutCubic));
        seq.Join(label.DOFade(0f, duration).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
