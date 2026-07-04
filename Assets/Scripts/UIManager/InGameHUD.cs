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
        RandomBoxIcon,
    }

    private const string OrderPrefabPath     = "UI/Slot/OrderSlot";
    private const string CoinFloatPrefabPath = "UI/HUD/HudFloatText";

    private int lastMoney = -1;   // 코인 증가분 감지용 (-1 = 초기화 전)

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

        // 럭키박스 버튼 — 코인 차감 성공 시 팝업 (RandomBoxManager.OnBoxOpened 경유)
        BindEvent(Get<GameObject>((int)GameObjects.RandomBoxIcon), evt =>
        {
            if (RandomBoxManager.Instance != null)
                RandomBoxManager.Instance.TryOpen();
        });
    }

    // 씬에 직접 배치된 경우 대비
    void Start() => Init();

    // ── 전역 HUD 이벤트 (static — 노션 규칙: 매니저급 UI 한 번 구독) ──
    private void OnEnable()
    {
        GameManager.OnMoneyChanged     += HandleMoneyChanged;
        GameManager.OnScoreChanged     += HandleScoreChanged;
        GameManager.OnTimeTick         += HandleTimeTick;
        RandomBoxManager.OnBoxOpened   += HandleBoxOpened;
    }

    private void OnDisable()
    {
        GameManager.OnMoneyChanged     -= HandleMoneyChanged;
        GameManager.OnScoreChanged     -= HandleScoreChanged;
        GameManager.OnTimeTick         -= HandleTimeTick;
        RandomBoxManager.OnBoxOpened   -= HandleBoxOpened;
    }

    // 랜덤박스 개봉 이벤트 → 팝업 표시 (UI는 구독자)
    private void HandleBoxOpened()
    {
        UIManager.Instance.ShowPopupUI<RandomBoxPopup>();
    }

    private void HandleMoneyChanged(int money)
    {
        // 변화량 플로팅 — 증가 초록 +N, 감소 빨강 -N (코인 카운터 위에서 떠오름)
        if (lastMoney >= 0 && money != lastMoney)
            SpawnCoinFloat(money - lastMoney);
        lastMoney = money;

        if (coinText != null) coinText.text = money.ToString();
    }

    private void SpawnCoinFloat(int delta)
    {
        if (coinText == null) return;

        var prefab = Resources.Load<GameObject>(CoinFloatPrefabPath);
        if (prefab == null) return;

        // 코인 카운터와 같은 부모에 스폰 → 같은 위치 기준으로 위로 떠오름
        var coinRect = (RectTransform)coinText.transform.parent;
        var go = Instantiate(prefab, coinRect.parent);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = coinRect.anchorMin;
        rt.anchorMax = coinRect.anchorMax;
        rt.anchoredPosition = coinRect.anchoredPosition;

        string text = delta > 0 ? $"+{delta}" : delta.ToString();   // 음수는 자체 '-' 포함
        Color  color = delta > 0
            ? new Color(0.35f, 0.9f, 0.35f)
            : new Color(0.9f, 0.3f, 0.3f);

        go.GetComponent<HudFloatText>().Show(text, color);
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
