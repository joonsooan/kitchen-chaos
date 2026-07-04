using UnityEngine;
using UnityEngine.UI;

public class UISlot : UIBase
{
    enum Images       { MenuImage }
    enum Sliders      { Gauge }
    enum GameObjects  { IngredientRow }

    private const string IngredientPrefabPath = "UI/Slot/IngredientSlot";

    private Image     _menuImage;
    private Slider    _gauge;
    private Transform _ingredientRow;

    private Customer _customer;   // 게이지·카드 수명 단일 소스

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));

        _menuImage     = Get<Image>((int)Images.MenuImage);
        _gauge         = Get<Slider>((int)Sliders.Gauge);
        _ingredientRow = Get<GameObject>((int)GameObjects.IngredientRow).transform;
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
    }

    // ── 이벤트 구독 (인스턴스 이벤트 — OnEnable/OnDisable 짝 규칙) ────
    private void OnEnable()  => Subscribe();
    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (_customer == null) return;

        // 중복 방지: -= 후 +=
        _customer.OnOrderSucceeded -= HandleOrderClosed;
        _customer.OnOrderFailed    -= HandleOrderClosed;
        _customer.OnOrderSucceeded += HandleOrderClosed;
        _customer.OnOrderFailed    += HandleOrderClosed;
    }

    private void Unsubscribe()
    {
        if (_customer == null) return;

        _customer.OnOrderSucceeded -= HandleOrderClosed;
        _customer.OnOrderFailed    -= HandleOrderClosed;
    }

    // 성공/실패 공통 — 주문 카드 제거
    private void HandleOrderClosed(Customer customer, RecipeData recipe)
    {
        Destroy(gameObject);
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
            if (icon != null) icon.sprite = entry.ingredientType.ingredientIcon;
        }
    }

    // ── 게이지 (RemainingPatience 폴링) ───────────────────────────
    private void Update()
    {
        if (_customer == null || _gauge == null) return;
        if (_customer.CurrentState != CustomerState.Waiting) return;

        float tolerance = _customer.CustomerData != null
            ? _customer.CustomerData.toleranceSeconds
            : 0f;
        if (tolerance <= 0f) return;

        _gauge.value = Mathf.Clamp01(_customer.RemainingPatience / tolerance);
    }
}
