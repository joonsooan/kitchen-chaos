using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameHUD : UIHUD
{
    enum Texts
    {
        TimeText,
        CoinText,
        ScoreText,
    }

    enum GameObjects
    {
        OrderLayout,
    }

    private const string OrderPrefabPath = "UI/Slot/OrderSlot";

    private TextMeshProUGUI timeText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI scoreText;
    private Transform orderLayout;
    private bool initialized;

    // ShowHUDUI가 호출 — AddOrder 전에 바인딩 보장 (Start는 한 프레임 늦어 순서버그)
    public override void Init()
    {
        if (initialized) return;
        initialized = true;

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        timeText    = Get<TextMeshProUGUI>((int)Texts.TimeText);
        coinText    = Get<TextMeshProUGUI>((int)Texts.CoinText);
        scoreText   = Get<TextMeshProUGUI>((int)Texts.ScoreText);
        orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    // 씬에 직접 배치된 경우 대비
    void Start() => Init();

    // ── 전역 HUD 이벤트 (static — 노션 규칙: 매니저급 UI 한 번 구독) ──
    private void OnEnable()
    {
        GameManager.OnMoneyChanged += HandleMoneyChanged;
        GameManager.OnScoreChanged += HandleScoreChanged;
        GameManager.OnTimeTick     += HandleTimeTick;
    }

    private void OnDisable()
    {
        GameManager.OnMoneyChanged -= HandleMoneyChanged;
        GameManager.OnScoreChanged -= HandleScoreChanged;
        GameManager.OnTimeTick     -= HandleTimeTick;
    }

    private void HandleMoneyChanged(int money)
    {
        if (coinText != null) coinText.text = money.ToString();
    }

    private void HandleScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    private void HandleTimeTick(float elapsedTime)
    {
        if (timeText == null) return;

        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // 주문 카드 추가 — 손님 착석 시 게임 로직에서 호출.
    // 카드 게이지·제거는 UISlot이 손님 이벤트로 처리.
    public UISlot AddOrder(Customer customer)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), orderLayout);
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
        slot.Setup(customer);
        return slot;
    }
}
