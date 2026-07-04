using UnityEngine;

// 유진 씬 테스트 — 손님 N명 스폰 + 착석 → 주문 카드 자동 생성·게이지·제거 검증
public class CustomerGaugeTest : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;          // Customer_Rabbit 등
    [SerializeField] private CustomerGaugeView gaugePrefab;    // CustomerGauge 프리팹 (월드 게이지)
    [SerializeField] private int count = 3;
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private float spacing = 2f;

    private void Start()
    {
        if (customerPrefab == null)
        {
            Debug.LogWarning("[CustomerGaugeTest] customerPrefab 미할당");
            return;
        }

        var hud = UIManager.Instance != null
            ? UIManager.Instance.ShowHUDUI<InGameHUD>()
            : null;

        // 전역 HUD 이벤트 소스 보장 (시간/돈/점수)
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();
        GameManager.Instance.StartGame();

        // 랜덤박스/버프 매니저 보장
        if (RandomBoxManager.Instance == null)
            new GameObject("RandomBoxManager").AddComponent<RandomBoxManager>();
        if (BuffManager.Instance == null)
            new GameObject("BuffManager").AddComponent<BuffManager>();

        var pool = DataTable.Customers;   // 손님 데이터 풀 (레시피 배분)

        for (int i = 0; i < count; i++)
        {
            var pos      = startPosition + Vector3.right * (spacing * i);
            var customer = Instantiate(customerPrefab, pos, Quaternion.identity);

            // 테스트 씬엔 GridSystem/SeatManager 없음 → 이동 로직 제거 (퇴장 경로 NRE 방지)
            var movement = customer.GetComponent<CustomerMovement>();
            if (movement != null) Destroy(movement);

            if (pool != null && pool.Length > 0)
                customer.SetData(pool[i % pool.Length]);

            customer.Seat();   // Waiting 시작 → 카드 게이지 카운트다운

            // 손님 머리 위 월드 게이지 (주문 카드와 동일 소스)
            if (gaugePrefab != null)
                Instantiate(gaugePrefab).Bind(customer);

            // 주문 카드 (게이지·제거는 UISlot이 손님 이벤트로 처리)
            if (hud != null) hud.AddOrder(customer);
        }
    }
}
