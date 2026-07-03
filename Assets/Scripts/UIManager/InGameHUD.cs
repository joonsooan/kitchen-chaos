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

<<<<<<< HEAD
    private const string OrderPrefabPath = "UI/Slot/OrderSlot";

    private TextMeshProUGUI timeText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI scoreText;
    private Transform orderLayout;
    private float elapsed;
=======
    private const string OrderPrefabPath = "UI/Slot/Order";
    private bool initialized;

    private TextMeshProUGUI TimeText;
    private Transform _orderLayout;
    private float _elapsed;
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
    // ShowHUDUI가 호출 — AddOrder 전에 바인딩 보장 (Start는 한 프레임 늦어 순서버그)
    public override void Init()
    {
        if (initialized) return;
        initialized = true;

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

<<<<<<< HEAD
        timeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
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
=======
        TimeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        _orderLayout = Get<GameObject>((int)GameObjects.OrderLayout).transform;
    }

    // 주문 1개 추가 — 게임 로직에서 호출
    public UISlot AddOrder(RecipeData recipe)
    {
        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), _orderLayout);
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
<<<<<<< HEAD
        slot.Setup(customer);
=======
        slot.Setup(recipe);
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
        return slot;
    }

    // Update is called once per frame
    void Update()
    {
        //시간따라 mm:ss
<<<<<<< HEAD
        elapsed += Time.deltaTime;

        int minutes = (int)(elapsed / 60f);
        int seconds = (int)(elapsed % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    // 풀에서 랜덤 손님 1명으로 주문 추가 (test용도)
    public void AddOrder()
    {
        var customers = DataTable.Customers;
        if (customers == null || customers.Length == 0) return;

        var customer = customers[Random.Range(0, customers.Length)];
        AddOrder(customer);
=======
        _elapsed += Time.deltaTime;

        int minutes = (int)(_elapsed / 60f);
        int seconds = (int)(_elapsed % 60f);
        TimeText.text = $"{minutes:00}:{seconds:00}";
        
        
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
    }
}
