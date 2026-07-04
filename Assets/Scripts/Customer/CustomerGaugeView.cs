using UnityEngine;
using UnityEngine.UI;

// 손님 위에 뜨는 월드 UI 게이지 — 남은 인내심 표시 (주문 카드 게이지와 동일 소스)
public class CustomerGaugeView : MonoBehaviour
{
    [SerializeField] private Image fill;                                  // 채움 이미지 (pivot 왼쪽)
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    private Customer target;
    private Camera cam;

    public void Bind(Customer customer)
    {
        target = customer;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null) { Destroy(gameObject); return; }

        // 손님 위로 위치
        transform.position = target.transform.position + worldOffset;

        // 카메라 향해 빌보드
        if (cam != null)
            transform.forward = cam.transform.forward;

        // 남은 인내심 반영 — fill(pivot 왼쪽)을 x스케일로 채움 (Image 타입 무관)
        float tolerance = target.CustomerData != null
            ? target.CustomerData.toleranceSeconds
            : 0f;
        if (fill != null && tolerance > 0f)
        {
            float t = Mathf.Clamp01(target.RemainingPatience / tolerance);
            var s = fill.rectTransform.localScale;
            s.x = t;
            fill.rectTransform.localScale = s;
            fill.color = UISlot.GaugeColor(t);   // 주문 카드 게이지와 동일 색 규칙
        }

        // 대기 종료(성공/실패/퇴장) 시 게이지 제거
        if (target.CurrentState == CustomerState.LeavingSuccess ||
            target.CurrentState == CustomerState.LeavingFailure)
        {
            Destroy(gameObject);
        }
    }
}
