using UnityEngine;
using UnityEngine.UI;

// 주문 카드의 재료 한 칸 — 재료 아이콘 + 아래 조리방법 아이콘.
// 조리방법 스프라이트 매핑은 프리팹에 직렬화 (index = (int)CookingMethod).
public class RecipeSlotView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image methodIcon;
    [SerializeField] private Sprite[] methodIcons;   // None, Fry, Chop, Mix, Boil

    public void Setup(IngredientEntry entry)
    {
        if (entry.ingredientType == null) return;

        if (icon != null)
        {
            var combined = entry.ingredientType.GetCookingMethodIcon(entry.requiredCookingMethod);
            icon.sprite = combined != null ? combined : entry.ingredientType.ingredientIcon;
        }

        if (methodIcon == null) return;

        int index = (int)entry.requiredCookingMethod;
        Sprite sprite = (methodIcons != null && index > 0 && index < methodIcons.Length)
            ? methodIcons[index]
            : null;

        methodIcon.sprite = sprite;
        methodIcon.gameObject.SetActive(sprite != null);   // None/미지정은 숨김
    }
}
