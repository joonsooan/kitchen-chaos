using UnityEngine;
using UnityEngine.UI;

// 몬스터(양배추·잡초) 머리 위 월드공간 HP 바 — 첫 피격 시 노출, 대상 파괴 시 자가 제거.
// 손님 인내심 게이지(CustomerGaugeView)와 동일 패턴: fill(pivot 왼쪽)을 x스케일로 채움 + 카메라 빌보드.
public class MonsterHPBarView : MonoBehaviour
{
    [SerializeField] private Image fill;                               // 채움 이미지 (pivot 왼쪽)
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.7f, 0f);

    private Transform target;
    private IHasHealth health;
    private Camera cam;

    // 몬스터가 첫 피격 시 호출. Resources/UI/World/MonsterHPBar 프리팹을 띄우고 대상에 바인딩.
    public static void Show(Component source)
    {
        if (source == null) return;

        var prefab = Resources.Load<GameObject>("UI/World/MonsterHPBar");
        if (prefab == null) return;

        var view = Object.Instantiate(prefab).GetComponent<MonsterHPBarView>();
        if (view != null) view.Bind(source);
    }

    public void Bind(Component source)
    {
        target = source != null ? source.transform : null;
        health = source as IHasHealth;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || health == null) { Destroy(gameObject); return; }   // 몬스터 사망 시 제거

        // 몬스터 위로 위치 + 카메라 향해 빌보드
        transform.position = target.position + worldOffset;
        if (cam != null) transform.forward = cam.transform.forward;

        // 남은 체력 반영 — fill(pivot 왼쪽)을 x스케일로 채움
        if (fill != null && health.MaxHealth > 0)
        {
            float t = Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth);
            var s = fill.rectTransform.localScale;
            s.x = t;
            fill.rectTransform.localScale = s;
            fill.color = UISlot.GaugeColor(t);   // 게이지 공용 색 규칙(초록→노랑→빨강)
        }
    }
}
