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
        Destroy(gameObject);
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
