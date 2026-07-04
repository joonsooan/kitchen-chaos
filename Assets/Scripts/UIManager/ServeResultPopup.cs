using DG.Tweening;
using TMPro;
using UnityEngine;

// 서빙 결과 플로팅 텍스트 — 테이블 위에서 떠오르며 페이드아웃 후 파괴
public class ServeResultPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float rise     = 1.2f;
    [SerializeField] private float duration = 1.0f;

    public void Show(Vector3 worldPos, string text, Color color)
    {
        transform.position = worldPos;
        label.text  = text;
        label.color = color;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(worldPos.y + rise, duration).SetEase(Ease.OutCubic));
        seq.Join(label.DOFade(0f, duration).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}
