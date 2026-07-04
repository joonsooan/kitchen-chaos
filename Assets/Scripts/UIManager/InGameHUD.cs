using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class InGameHUD : UIHUD
{
    enum Texts
    {
        TimeText,
        CoinText,
        ScoreText,
        BuffTimeText,
    }

    enum GameObjects
    {
        OrderLayout,
        RandomBoxIcon,
        BuffIcon,
        SettingIcon,
    }

    private const string OrderPrefabPath     = "UI/Slot/OrderSlot";
    private const string CoinFloatPrefabPath = "UI/HUD/HudFloatText";

    private int lastMoney = -1;   // 코인 증가분 감지용 (-1 = 초기화 전)
    private int lastScore = -1;
    private DG.Tweening.Tween coinRollTween;
    private DG.Tweening.Tween scoreRollTween;
    private readonly HashSet<BuffData> activeBuffs = new();   // 동시·재획득 버프 추적
    private readonly Dictionary<Customer, UISlot> activeSlots = new();   // 손님당 카드 1개
    private BuffData lastBuff;    // 남은시간 표시 대상 (가장 최근 버프)

    // 날아가는 동전 목표 지점 (ServeResultView가 사용)
    public RectTransform CoinAnchor => coinText != null ? coinText.rectTransform : null;

    private TextMeshProUGUI timeText;
    private TextMeshProUGUI coinText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI buffTimeText;
    private Transform orderLayout;
    private GameObject buffIcon;
    private Transform randomBoxIcon;
    private Tween boxBounceTween;
    private UnityEngine.UI.Image buffIconImage;
    private bool initialized;

    // ShowHUDUI가 호출 — AddOrder 전에 바인딩 보장 (Start는 한 프레임 늦어 순서버그)
    public override void Init()
    {
        if (initialized) return;
        initialized = true;

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        timeText     = Get<TextMeshProUGUI>((int)Texts.TimeText);
        coinText     = Get<TextMeshProUGUI>((int)Texts.CoinText);
        scoreText    = Get<TextMeshProUGUI>((int)Texts.ScoreText);
        buffTimeText = Get<TextMeshProUGUI>((int)Texts.BuffTimeText);
        orderLayout  = Get<GameObject>((int)GameObjects.OrderLayout).transform;
        buffIcon     = Get<GameObject>((int)GameObjects.BuffIcon);
        if (buffIcon != null) buffIconImage = buffIcon.GetComponent<UnityEngine.UI.Image>();

        randomBoxIcon = Get<GameObject>((int)GameObjects.RandomBoxIcon).transform;

        // 럭키박스 버튼 — 코인 차감 성공 시 팝업 (RandomBoxManager.OnBoxOpened 경유)
        BindEvent(Get<GameObject>((int)GameObjects.RandomBoxIcon), evt =>
        {
            if (RandomBoxManager.Instance != null)
                RandomBoxManager.Instance.TryOpen();
        });

        // 설정 버튼 — 설정 팝업 (BGM/SFX)
        BindEvent(Get<GameObject>((int)GameObjects.SettingIcon), evt =>
        {
            UIManager.Instance.ShowPopupUI<OptionPopup>();
        });

        PlayIntroFx();
    }

    // J. HUD 요소 스태거 입장 — 순차적으로 뿅뿅 자리 잡음
    public void PlayIntroFx()
    {
        Transform[] targets =
        {
            timeText  != null ? timeText.transform.parent : null,
            coinText  != null ? coinText.transform.parent : null,
            scoreText != null ? scoreText.transform.parent : null,
            randomBoxIcon,
            Get<GameObject>((int)GameObjects.SettingIcon)?.transform,
        };

        float delay = 0f;
        foreach (var t in targets)
        {
            if (t == null) continue;

            t.localScale = Vector3.zero;
            t.DOScale(1f, 0.28f)
             .SetDelay(delay)
             .SetEase(Ease.OutBack)
             .SetLink(t.gameObject);
            delay += 0.08f;
        }
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
        BuffManager.OnBuffStarted      += HandleBuffStarted;
        BuffManager.OnBuffEnded        += HandleBuffEnded;
        TableServing.OnDishServed      += HandleDishServed;
    }

    private void OnDisable()
    {
        GameManager.OnMoneyChanged     -= HandleMoneyChanged;
        GameManager.OnScoreChanged     -= HandleScoreChanged;
        GameManager.OnTimeTick         -= HandleTimeTick;
        RandomBoxManager.OnBoxOpened   -= HandleBoxOpened;
        BuffManager.OnBuffStarted      -= HandleBuffStarted;
        BuffManager.OnBuffEnded        -= HandleBuffEnded;
        TableServing.OnDishServed      -= HandleDishServed;
    }

    // 버프 시작/종료 → BuffIcon 표시 토글 (재획득·동시 버프는 HashSet으로 정확 추적)
    private void HandleBuffStarted(BuffData buff)
    {
        activeBuffs.Add(buff);
        lastBuff = buff;
        if (buffIcon == null) return;

        buffIcon.SetActive(true);

        // L. 획득 순간 뿅 등장
        buffIcon.transform.DOKill();
        buffIcon.transform.localScale = Vector3.zero;
        buffIcon.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetLink(buffIcon);

        // 버프 아이콘 스프라이트 지정돼 있으면 교체 (없으면 기존 이미지 유지)
        if (buff.icon != null && buffIcon.TryGetComponent(out UnityEngine.UI.Image image))
            image.sprite = buff.icon;
    }

    private void HandleBuffEnded(BuffData buff)
    {
        activeBuffs.Remove(buff);
        if (activeBuffs.Count == 0 && buffIcon != null)
            buffIcon.SetActive(false);
    }

    // 남은시간 폴링 — BuffIcon 밑 BuffTimeText 갱신 + 만료 5초 전 깜빡
    private void Update()
    {
        UpdateBoxBounce();

        if (buffTimeText == null || lastBuff == null || !buffIcon.activeSelf) return;
        if (BuffManager.Instance == null) return;

        float remaining = BuffManager.Instance.GetRemaining(lastBuff);
        buffTimeText.text = Mathf.CeilToInt(remaining).ToString();

        // 만료 임박 — 아이콘·시간 깜빡 (알파 진동)
        if (buffIconImage != null)
        {
            float alpha = remaining <= 5f
                ? 0.35f + 0.65f * Mathf.PingPong(Time.time * 5f, 1f)
                : 1f;
            var c = buffIconImage.color; c.a = alpha; buffIconImage.color = c;
            var tc = buffTimeText.color; tc.a = alpha; buffTimeText.color = tc;
        }
    }

    // 살 수 있을 때 럭키박스 아이콘 살랑살랑 (어포던스)
    private void UpdateBoxBounce()
    {
        if (randomBoxIcon == null || GameManager.Instance == null || RandomBoxManager.Instance == null) return;

        bool affordable = GameManager.Instance.Money >= RandomBoxManager.Instance.Cost;
        bool bouncing   = boxBounceTween != null && boxBounceTween.IsActive();

        if (affordable && !bouncing)
        {
            boxBounceTween = randomBoxIcon
                .DOScale(1.12f, 0.45f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(randomBoxIcon.gameObject);
        }
        else if (!affordable && bouncing)
        {
            boxBounceTween.Kill();
            boxBounceTween = null;
            randomBoxIcon.localScale = Vector3.one;
        }
    }

    // 서빙 판정 즉시 — 해당 손님 카드에 도장 (제거 연출은 손님 이벤트가 이어서)
    private void HandleDishServed(Table table, Customer customer, RecipeData recipe, bool succeeded)
    {
        if (customer != null && activeSlots.TryGetValue(customer, out var slot) && slot != null)
            slot.ShowStamp(succeeded);
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

        // 숫자 롤링 카운트업 + 도착 펀치
        RollNumber(coinText, lastMoney < 0 ? money : lastMoney, money, ref coinRollTween);
        lastMoney = money;
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
        RollNumber(scoreText, lastScore < 0 ? score : lastScore, score, ref scoreRollTween);
        lastScore = score;
    }

    // 숫자 롤링 — from→to 착착 올라가고 도착 시 펀치
    private void RollNumber(TMPro.TextMeshProUGUI label, int from, int to, ref DG.Tweening.Tween tween)
    {
        if (label == null) return;

        tween?.Kill();
        if (from == to) { label.text = to.ToString(); return; }

        int shown = from;
        tween = DG.Tweening.DOTween.To(() => shown, x => { shown = x; label.text = x.ToString(); }, to, 0.4f)
            .SetEase(DG.Tweening.Ease.OutQuad)
            .SetLink(label.gameObject)
            .OnComplete(() =>
            {
                label.transform.DOKill();
                label.transform.localScale = Vector3.one;
                label.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 6, 0.6f)
                    .SetLink(label.gameObject);
            });
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
        // 같은 손님 카드가 이미 살아있으면 중복 생성 방지
        if (activeSlots.TryGetValue(customer, out var existing) && existing != null)
            return existing;

        var go   = Instantiate(Resources.Load<GameObject>(OrderPrefabPath), orderLayout);
        var slot = go.GetComponent<UISlot>();
        if (slot == null)
        {
            slot = go.AddComponent<UISlot>();
        }
        slot.Init();
        slot.Setup(customer);
        activeSlots[customer] = slot;
        return slot;
    }
}
