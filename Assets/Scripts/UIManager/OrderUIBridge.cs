<<<<<<< HEAD
using UnityEngine;

// 실게임 배선 브리지 — 손님 착석 시 주문 카드 + 월드 게이지 자동 연결.
// Customer의 static 이벤트 구독 (씬에 하나만 두면 됨).
public class OrderUIBridge : MonoBehaviour
{
    [SerializeField] private CustomerGaugeView gaugePrefab;

    private InGameHUD hud;
=======
using System.Collections.Generic;
using UnityEngine;

// 실게임 배선 브리지 — 손님이 Waiting 진입하면 주문 카드 + 월드 게이지 자동 연결.
// CustomerSpawner를 건드리지 않고 주기 스캔으로 신규 손님 감지.
public class OrderUIBridge : MonoBehaviour
{
    [SerializeField] private CustomerGaugeView gaugePrefab;
    [SerializeField] private float scanInterval = 0.5f;

    private readonly HashSet<Customer> tracked = new();
    private InGameHUD hud;
    private float scanTimer;
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)

    private void Start()
    {
        if (UIManager.Instance != null)
            hud = UIManager.Instance.ShowHUDUI<InGameHUD>();

        // 시간/보상 흐름 시작 (게임 시작 로직 생기면 그쪽으로 이동)
        if (GameManager.Instance != null)
<<<<<<< HEAD
        {
            GameManager.Instance.StartGame();
            GameManager.Instance.AddMoney(100);   // TEST: 랜덤박스 테스트용 — 확인 후 제거
        }
    }

    private void OnEnable()  => Customer.OnAnyCustomerSeated += HandleSeated;
    private void OnDisable() => Customer.OnAnyCustomerSeated -= HandleSeated;

    private void HandleSeated(Customer customer)
=======
            GameManager.Instance.StartGame();
    }

    private void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer > 0f) return;
        scanTimer = scanInterval;

        foreach (var customer in FindObjectsByType<Customer>(FindObjectsSortMode.None))
        {
            if (!tracked.Add(customer)) continue;

            var captured = customer;
            captured.OnStateChanged += state =>
            {
                if (state == CustomerState.Waiting) Attach(captured);
            };

            if (captured.CurrentState == CustomerState.Waiting)
                Attach(captured);
        }
    }

    private void Attach(Customer customer)
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
    {
        if (hud != null) hud.AddOrder(customer);
        if (gaugePrefab != null) Instantiate(gaugePrefab).Bind(customer);
    }
}
