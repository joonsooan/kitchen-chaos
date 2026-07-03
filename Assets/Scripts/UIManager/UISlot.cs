using UnityEngine;
using UnityEngine.UI;

public class UISlot : UIBase
{
    enum Images       { MenuImage }
<<<<<<< HEAD
    enum Sliders      { Gauge }
    enum GameObjects  { IngredientRow }

    private const string IngredientPrefabPath = "UI/Slot/IngredientSlot";

    private Image     _menuImage;
    private Slider    _gauge;
    private Transform _ingredientRow;

    private float _tolerance;
    private float _remaining;
    private bool  _counting;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Slider>(typeof(Sliders));
        Bind<GameObject>(typeof(GameObjects));

        _menuImage     = Get<Image>((int)Images.MenuImage);
        _gauge         = Get<Slider>((int)Sliders.Gauge);
        _ingredientRow = Get<GameObject>((int)GameObjects.IngredientRow).transform;
    }

    // CustomerData로 카드 채우기 — 레시피 표시 + 게이지(toleranceSeconds) 시작
    public void Setup(CustomerData customer)
    {
        if (customer == null) return;

        FillRecipe(customer.requiredRecipe);
        StartGauge(customer.toleranceSeconds);
    }

    private void FillRecipe(RecipeData recipe)
=======
    enum GameObjects  { IngredientRow }

    private const string IngredientPrefabPath = "UI/Slot/Ingredient";

    private Image     _menuImage;
    private Transform _ingredientRow;

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        _menuImage     = Get<Image>((int)Images.MenuImage);
        _ingredientRow = Get<GameObject>((int)GameObjects.IngredientRow).transform;
    }

    // RecipeData로 카드 내용 채우기
    public void Setup(RecipeData recipe)
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
    {
        if (recipe == null) return;

        _menuImage.sprite = recipe.recipeIcon;

        // 기존 재료 아이콘 제거
        for (int i = _ingredientRow.childCount - 1; i >= 0; i--)
            Destroy(_ingredientRow.GetChild(i).gameObject);

<<<<<<< HEAD
        // 레시피 재료마다 IngredientSlot 스폰
=======
        // 레시피 재료마다 Ingredient 스폰
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
        var prefab = Resources.Load<GameObject>(IngredientPrefabPath);
        foreach (var entry in recipe.ingredients)
        {
            if (entry.ingredientType == null) continue;

            var go   = Instantiate(prefab, _ingredientRow);
            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null) icon.sprite = entry.ingredientType.ingredientIcon;
<<<<<<< HEAD
        }
    }

    private void StartGauge(float tolerance)
    {
        _tolerance = Mathf.Max(0.01f, tolerance);
        _remaining = _tolerance;
        _counting  = true;
        SetGauge(1f);
    }

    // 게이지 직접 세팅 (0~1) — 외부에서 실제 손님 타이머 연결 시 사용
    public void SetGauge(float value01)
    {
        if (_gauge != null) _gauge.value = Mathf.Clamp01(value01);
    }

    private void Update()
    {
        if (!_counting) return;

        _remaining -= Time.deltaTime;
        SetGauge(_remaining / _tolerance);

        if (_remaining <= 0f) _counting = false;
=======

            // 조리방법 데이터 없음 → 숨김
            var method = go.transform.Find("method");
            if (method != null) method.gameObject.SetActive(false);
        }
>>>>>>> 75ee4b2 (add: 인게임 HUD 모양잡기)
    }
}
