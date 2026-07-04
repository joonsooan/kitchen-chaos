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
        {
            hud = UIManager.Instance.ShowHUDUI<InGameHUD>();
            UIManager.Instance.HideHUDUI<InGameHUD>();   // 게임 시작 전엔 HUD 숨김

            // 타이틀(홈)~이름 입력 동안 게임 전체 정지 (스포너·이동 포함)
            Time.timeScale = 0f;
            UIManager.Instance.ShowHUDUI<HomeHUD>();
            SoundManager.Instance?.SetPhase(GamePhase.MainMenu);   // 홈~스테이지 진입 전까지 메인메뉴 BGM
        }

        // 퇴식구마다 남은 그릇 수 표시 부착
        var stockPrefab = Resources.Load<GameObject>("UI/World/StockCount");
        if (stockPrefab != null)
        {
            foreach (var station in FindObjectsByType<ReturnStation>(FindObjectsSortMode.None))
                Instantiate(stockPrefab).GetComponent<StockCountView>().Bind(station);
        }
    }

    private void OnEnable()
    {
        Customer.OnAnyCustomerSeated       += HandleSeated;
        ReturnStation.OnDispenseFailedEmpty += HandleStationEmpty;
        HomeHUD.OnStartRequested           += HandleHomeStart;
        TutorialPopup.OnTutorialCompleted  += HandleTutorialCompleted;
        NameInputPopup.OnNameConfirmed     += HandleNameConfirmed;
    }

    private void OnDisable()
    {
        Customer.OnAnyCustomerSeated       -= HandleSeated;
        ReturnStation.OnDispenseFailedEmpty -= HandleStationEmpty;
        HomeHUD.OnStartRequested           -= HandleHomeStart;
        TutorialPopup.OnTutorialCompleted  -= HandleTutorialCompleted;
        NameInputPopup.OnNameConfirmed     -= HandleNameConfirmed;
    }

    // 타이틀 시작 버튼 → 튜토리얼
    private void HandleHomeStart()
    {
        UIManager.Instance.ShowPopupUI<TutorialPopup>();
    }

    // 튜토리얼 완료 → 이름 입력 팝업
    private void HandleTutorialCompleted()
    {
        UIManager.Instance.ShowPopupUI<NameInputPopup>();
    }

    // 이름 확정 → 게임 재개 + 시간/보상 흐름 시작
    private void HandleNameConfirmed()
    {
        UIManager.Instance.ShowHUDUI<InGameHUD>();   // HUD 등장 (스태거 인트로 재생)
        if (hud != null) hud.PlayIntroFx();

        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    // 컵/접시 재고 소진 상태에서 꺼내기 시도 → 건물 위 경고 플로팅
    public void HandleStationEmpty(ReturnStation station)
    {
        var prefab = Resources.Load<GameObject>("UI/World/ServeResultPopup");
        if (prefab == null || station == null) return;

        string message = station.ContainerType == CarryingItemType.Cup
            ? "컵이 모두 나가 있어요!"
            : "접시가 모두 나가 있어요!";
        var popup = Instantiate(prefab).GetComponent<ServeResultPopup>();
        popup.Show(station.transform.position + new Vector3(0f, 1.0f, 0f),
                   message, Color.black);
    }

    private void HandleSeated(Customer customer)
    {
        if (hud != null) hud.AddOrder(customer);
        if (gaugePrefab != null) Instantiate(gaugePrefab).Bind(customer);

        // 머리 위 주문 말풍선 (음식 조달/떠날 때까지 유지 — OrderBubbleView가 수명 관리)
        var bubblePrefab = Resources.Load<GameObject>("UI/World/OrderBubble");
        if (bubblePrefab != null)
            Instantiate(bubblePrefab).GetComponent<OrderBubbleView>().Bind(customer);

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
