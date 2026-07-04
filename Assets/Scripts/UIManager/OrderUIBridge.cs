using UnityEngine;

// 실게임 배선 브리지 — 손님 착석 시 주문 카드 + 월드 게이지 자동 연결.
// Customer의 static 이벤트 구독 (씬에 하나만 두면 됨).
public class OrderUIBridge : MonoBehaviour
{
    [SerializeField] private CustomerGaugeView gaugePrefab;

    private InGameHUD hud;

    private void Start()
    {
        if (UIManager.Instance != null)
            hud = UIManager.Instance.ShowHUDUI<InGameHUD>();

        // 시간/보상 흐름 시작 (게임 시작 로직 생기면 그쪽으로 이동)
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    private void OnEnable()  => Customer.OnAnyCustomerSeated += HandleSeated;
    private void OnDisable() => Customer.OnAnyCustomerSeated -= HandleSeated;

    private void HandleSeated(Customer customer)
    {
        if (hud != null) hud.AddOrder(customer);
        if (gaugePrefab != null) Instantiate(gaugePrefab).Bind(customer);

        // 착석 신호 — 머리 위 "!" 팝
        var prefab = Resources.Load<GameObject>("UI/World/ServeResultPopup");
        if (prefab != null)
        {
            var popup = Instantiate(prefab).GetComponent<ServeResultPopup>();
            popup.Show(customer.transform.position + new Vector3(0f, 1.6f, 0f),
                       "!", new Color(1f, 0.85f, 0.2f));
        }
    }
}
