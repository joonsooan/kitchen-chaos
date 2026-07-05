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
    private Tween boxGlowTween;
    private Tween boxWiggleTween;
    private Transform boxGlow;      // 살 수 있을 때 후광 (프리팹 optional)
    private UnityEngine.UI.Image buffIconImage;
    private bool initialized;
    private TextMeshProUGUI phaseLabelText;   // 페이즈/쉬는시간 탭 (프리팹 optional)
    private TextMeshProUGUI prankText;        // 장난 남은시간 (프리팹 optional)
    private TextMeshProUGUI targetText;       // 목표 점수/기한 (프리팹 optional)
    private float prankEndTime = -1f;         // Time.time 기준

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
        boxGlow       = transform.Find("BoxGlow");

        if (timeText != null)
        {
            var box = timeText.transform.parent;
            phaseLabelText = box.Find("PhaseTab/PhaseLabelText")?.GetComponent<TextMeshProUGUI>();
            prankText      = box.Find("PrankText")?.GetComponent<TextMeshProUGUI>();
        }

        if (scoreText != null)
            targetText = scoreText.transform.parent.Find("TargetText")?.GetComponent<TextMeshProUGUI>();

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
        DisasterEvent.OnAnyDisasterTriggered += HandlePrankStarted;
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
        DisasterEvent.OnAnyDisasterTriggered -= HandlePrankStarted;
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
            // 통통 바운스 (기존보다 크게)
            boxBounceTween = randomBoxIcon
                .DOScale(1.18f, 0.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(randomBoxIcon.gameObject);

            // 주기적 좌우 흔들기 — 1.4초마다 도리도리
            boxWiggleTween = randomBoxIcon
                .DOShakeRotation(0.5f, new Vector3(0f, 0f, 14f), 14)
                .SetDelay(0.9f)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(randomBoxIcon.gameObject);

            // 후광 펄스 — 노란 빛이 커졌다 작아졌다
            if (boxGlow != null)
            {
                boxGlow.gameObject.SetActive(true);
                boxGlow.localScale = Vector3.one * 0.9f;
                boxGlowTween = boxGlow
                    .DOScale(1.25f, 0.6f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(boxGlow.gameObject);
            }
        }
        else if (!affordable && bouncing)
        {
            boxBounceTween.Kill();
            boxBounceTween = null;
            boxWiggleTween?.Kill();
            boxWiggleTween = null;
            randomBoxIcon.localScale = Vector3.one;
            randomBoxIcon.localRotation = Quaternion.identity;

            if (boxGlow != null)
            {
                boxGlowTween?.Kill();
                boxGlowTween = null;
                boxGlow.gameObject.SetActive(false);
            }
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
        SoundManager.Instance?.PlaySFX(SFXType.RandomBoxOpen);
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

    // 장난(재앙) 발동 → 남은시간 추적 시작
    private void HandlePrankStarted(DisasterEvent prank)
    {
        if (prank != null && prank.Duration > 0f)
            prankEndTime = Time.time + prank.Duration;
    }

    private void HandleTimeTick(float elapsedTime)
    {
        if (timeText == null) return;

        // 재앙 기준 페이즈 (DisasterManager) — 구간 남은 시간, 없으면 기존 누적 시간
        var disaster = DisasterManager.Instance;
        float shown = disaster != null ? disaster.SegmentRemaining : elapsedTime;
        int minutes = (int)(shown / 60f);
        int seconds = (int)(shown % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";

        if (phaseLabelText != null && disaster != null)
            phaseLabelText.text = disaster.IsResting ? "쉬는 시간" : $"{disaster.CurrentPhase}페이즈";

        // 목표 시각화 — 페이즈 중엔 이번 판정 목표, 휴식 중엔 다음 페이즈 목표
        if (targetText != null)
        {
            bool show = disaster != null;
            if (targetText.gameObject.activeSelf != show) targetText.gameObject.SetActive(show);
            if (show)
                targetText.text = $"목표 점수 : {disaster.CurrentTargetScore}점";
        }

        // 장난 남은시간 — 끝나면(사라지면) 같이 사라짐
        if (prankText != null)
        {
            float remain = prankEndTime - Time.time;
            bool show = remain > 0f;
            if (prankText.gameObject.activeSelf != show) prankText.gameObject.SetActive(show);
            if (show)
            {
                int pm = (int)(remain / 60f);
                int ps = (int)(remain % 60f);
                prankText.text = $"장난 {pm:00}:{ps:00} 남음";
            }
        }
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
