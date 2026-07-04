<<<<<<< HEAD
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
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
using UnityEngine;

// 실게임 배선 브리지 — 손님 착석 시 주문 카드 + 월드 게이지 자동 연결.
// Customer의 static 이벤트 구독 (씬에 하나만 두면 됨).
public class OrderUIBridge : MonoBehaviour
{
    [SerializeField] private CustomerGaugeView gaugePrefab;

    private InGameHUD hud;
<<<<<<< HEAD
    private float scanTimer;
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)

    private void Start()
    {
        if (UIManager.Instance != null)
            hud = UIManager.Instance.ShowHUDUI<InGameHUD>();

        // 시간/보상 흐름 시작 (게임 시작 로직 생기면 그쪽으로 이동)
        if (GameManager.Instance != null)
<<<<<<< HEAD
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
=======
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
        {
            GameManager.Instance.StartGame();
            GameManager.Instance.AddMoney(100);   // TEST: 랜덤박스 테스트용 — 확인 후 제거
        }
    }

<<<<<<< HEAD
    private void Attach(Customer customer)
>>>>>>> e0ddd75 (feat(ui): add popups, buff system, world gauge, integration scene)
=======
    private void OnEnable()  => Customer.OnAnyCustomerSeated += HandleSeated;
    private void OnDisable() => Customer.OnAnyCustomerSeated -= HandleSeated;

    private void HandleSeated(Customer customer)
>>>>>>> 05c4587 (add: Scene_YujinTest에 ui배선 완료)
    {
        if (hud != null) hud.AddOrder(customer);
        if (gaugePrefab != null) Instantiate(gaugePrefab).Bind(customer);
    }
}
