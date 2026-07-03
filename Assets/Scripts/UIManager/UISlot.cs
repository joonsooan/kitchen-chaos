using UnityEngine;
using UnityEngine.UI;

public class UISlot : UIBase
{
    enum Images       { MenuImage }
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
    {
        if (recipe == null) return;

        _menuImage.sprite = recipe.recipeIcon;

        // 기존 재료 아이콘 제거
        for (int i = _ingredientRow.childCount - 1; i >= 0; i--)
            Destroy(_ingredientRow.GetChild(i).gameObject);

        // 레시피 재료마다 Ingredient 스폰
        var prefab = Resources.Load<GameObject>(IngredientPrefabPath);
        foreach (var entry in recipe.ingredients)
        {
            if (entry.ingredientType == null) continue;

            var go   = Instantiate(prefab, _ingredientRow);
            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null) icon.sprite = entry.ingredientType.ingredientIcon;

            // 조리방법 데이터 없음 → 숨김
            var method = go.transform.Find("method");
            if (method != null) method.gameObject.SetActive(false);
        }
    }
}
