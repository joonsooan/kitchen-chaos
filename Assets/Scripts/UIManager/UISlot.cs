using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : UIBase
{
    enum Images       { MenuImage }
    enum Sliders      { Gauge }
    enum GameObjects  { IngredientRow }

    private const string IngredientPrefabPath = "UI/Slot/IngredientSlot";

    private static readonly Color GaugeGreen  = new Color(0.35f, 0.85f, 0.35f);
    private static readonly Color GaugeYellow = new Color(0.95f, 0.8f, 0.25f);
    private static readonly Color GaugeRed    = new Color(0.9f, 0.25f, 0.25f);

    private Image     _menuImage;
    private Slider    _gauge;
    private Image     _gaugeFill;
    private Transform _ingredientRow;

    private Customer _customer;   // 게이지·카드 수명 단일 소스
    private bool _closing;        // 퇴장 연출 중
    private bool _urgent;         // 긴급 흔들림 시작됨
    private int  _lastCountShown = -1;
    private TMPro.TextMeshProUGUI _countText;   // 3·2·1 카운트다운 (프리팹 optional)
    private TMPro.TextMeshProUGUI _stampText;   // Success/Fail 도장 (프리팹 optional)

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));

        _menuImage     = Get<Image>((int)Images.MenuImage);
        _gauge         = Get<Slider>((int)Sliders.Gauge);
        _ingredientRow = Get<GameObject>((int)GameObjects.IngredientRow).transform;

        if (_gauge != null && _gauge.fillRect != null)
            _gaugeFill = _gauge.fillRect.GetComponent<Image>();

        var count = transform.Find("CountText");
        if (count != null) _countText = count.GetComponent<TMPro.TextMeshProUGUI>();

        var stamp = transform.Find("StampText");
        if (stamp != null) _stampText = stamp.GetComponent<TMPro.TextMeshProUGUI>();
    }

    // 손님으로 카드 채우기 — 레시피 표시 + 성공/실패 이벤트 구독
    public void Setup(Customer customer)
    {
        if (customer == null) return;

        Unsubscribe();               // 재사용 대비
        _customer = customer;
        Subscribe();

        if (customer.CustomerData != null)
            FillRecipe(customer.CustomerData.requiredRecipe);

        PlayEnterFx();
    }

    // 등장 연출 — 스케일 팝 (위치는 절대 안 건드림: HLG 재배치와 충돌 방지)
    private void PlayEnterFx()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1f, 0.22f)
                 .SetEase(Ease.OutBack)
                 .SetLink(gameObject);
    }

    // ── 이벤트 구독 (인스턴스 이벤트 — OnEnable/OnDisable 짝 규칙) ────
    private void OnEnable()  => Subscribe();
    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (_customer == null) return;

        // 중복 방지: -= 후 +=
        _customer.OnOrderSucceeded -= HandleOrderSucceeded;
        _customer.OnOrderFailed    -= HandleOrderFailed;
        _customer.OnOrderSucceeded += HandleOrderSucceeded;
        _customer.OnOrderFailed    += HandleOrderFailed;
    }

    private void Unsubscribe()
    {
        if (_customer == null) return;

        _customer.OnOrderSucceeded -= HandleOrderSucceeded;
        _customer.OnOrderFailed    -= HandleOrderFailed;
    }

    // 서빙 판정 순간 도장 쾅 — 카드 제거 연출 전 즉시 피드백
    public void ShowStamp(bool success)
    {
        if (_stampText == null) return;

        _stampText.text  = success ? "Success!" : "Fail..";
        _stampText.color = success
            ? new Color(0.2f, 0.8f, 0.25f)
            : new Color(0.9f, 0.25f, 0.25f);

        _stampText.gameObject.SetActive(true);
        _stampText.transform.DOKill();
        _stampText.transform.localScale = Vector3.one * 2.2f;   // 크게서 쾅 내려찍힘
        _stampText.transform.DOScale(1f, 0.18f)
                  .SetEase(Ease.InCubic)
                  .SetLink(_stampText.gameObject);
    }

    // 성공 — 위로 날아가며 축소 소멸
    private void HandleOrderSucceeded(Customer customer, RecipeData recipe)
    {
        if (_closing) return;
        _closing = true;

        transform.DOKill();
        var seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(0f, 0.22f).SetEase(Ease.InBack));
        seq.OnComplete(() => Destroy(gameObject));
    }

    // 실패 — 회색으로 식으며 아래로 툭
    private void HandleOrderFailed(Customer customer, RecipeData recipe)
    {
        if (_closing) return;
        _closing = true;

        transform.DOKill();
        var seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOShakeRotation(0.2f, new Vector3(0f, 0f, 8f)));
        seq.Append(transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
        seq.OnComplete(() => Destroy(gameObject));

        // 회색 틴트
        foreach (var img in GetComponentsInChildren<Image>())
            img.DOColor(new Color(0.5f, 0.5f, 0.5f, img.color.a), 0.25f).SetLink(gameObject);
    }

    // ── 레시피 표시 ───────────────────────────────────────────────
    private void FillRecipe(RecipeData recipe)
    {
        if (recipe == null) return;

        _menuImage.sprite = recipe.recipeIcon;

        // 기존 재료 아이콘 제거
        for (int i = _ingredientRow.childCount - 1; i >= 0; i--)
            Destroy(_ingredientRow.GetChild(i).gameObject);

        // 레시피 재료마다 IngredientSlot 프리팹 스폰
        var prefab = Resources.Load<GameObject>(IngredientPrefabPath);
        foreach (var entry in recipe.ingredients)
        {
            if (entry.ingredientType == null) continue;

            var go   = Instantiate(prefab, _ingredientRow);
            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                var combinedIcon = entry.ingredientType.GetCookingMethodIcon(entry.requiredCookingMethod);
                icon.sprite = combinedIcon != null ? combinedIcon : entry.ingredientType.ingredientIcon;
            }
        }
    }

    // ── 게이지 (RemainingPatience 폴링 + 잔량 색: 초록→노랑→빨강) ───
    private void Update()
    {
        if (_closing) return;

        // 안전망 — 이벤트를 놓쳤어도 손님이 떠났으면(파괴/풀복귀/퇴장) 카드 정리
        if (_customer == null || !_customer.gameObject.activeInHierarchy)
        {
            HandleOrderFailed(_customer, null);
            return;
        }
        if (_customer.CurrentState == CustomerState.LeavingSuccess)
        {
            HandleOrderSucceeded(_customer, null);
            return;
        }
        if (_customer.CurrentState == CustomerState.LeavingFailure)
        {
            HandleOrderFailed(_customer, null);
            return;
        }

        if (_gauge == null) return;
        if (_customer.CurrentState != CustomerState.Waiting) return;

        float tolerance = _customer.CustomerData != null
            ? _customer.CustomerData.toleranceSeconds
            : 0f;
        if (tolerance <= 0f) return;

        float t = Mathf.Clamp01(_customer.RemainingPatience / tolerance);
        _gauge.value = t;

        if (_gaugeFill != null)
            _gaugeFill.color = GaugeColor(t);

        // 긴급 — 잔여 20% 이하부터 카드 부르르
        if (!_urgent && t <= 0.2f)
        {
            _urgent = true;
            // 위치 셰이크 금지 — HLG 재배치와 싸워 카드가 옛 자리에 고정됨. 회전만.
            transform.DOShakeRotation(1f, new Vector3(0f, 0f, 4f), 12)
                     .SetLoops(-1)
                     .SetLink(gameObject);
        }

        // 막판 3·2·1 카운트다운 (CountText 있을 때만)
        if (_countText != null)
        {
            float remain = _customer.RemainingPatience;
            if (remain <= 3f && remain > 0f)
            {
                int sec = Mathf.CeilToInt(remain);
                if (sec != _lastCountShown)
                {
                    _lastCountShown = sec;
                    _countText.gameObject.SetActive(true);
                    _countText.text = sec.ToString();
                    _countText.transform.DOKill();
                    _countText.transform.localScale = Vector3.one * 1.6f;
                    _countText.transform.DOScale(1f, 0.25f).SetLink(_countText.gameObject);
                }
            }
            else if (_countText.gameObject.activeSelf)
            {
                _countText.gameObject.SetActive(false);
            }
        }
    }

    // 잔량 비율 → 색 (>0.5 초록~노랑, <0.5 노랑~빨강)
    public static Color GaugeColor(float t)
    {
        return t > 0.5f
            ? Color.Lerp(GaugeYellow, GaugeGreen, (t - 0.5f) * 2f)
            : Color.Lerp(GaugeRed, GaugeYellow, t * 2f);
    }
}
