using UnityEngine;

[CreateAssetMenu(fileName = "Ingredient", menuName = "KitchenChaos/Ingredient")]
public class Ingredient : ScriptableObject
{
    public Sprite ingredientIcon;
    public string ingredientName;

    // index = (int)CookingMethod. 재료 x 조리법 조합별 합성 이미지. 비어있으면 ingredientIcon으로 fallback.
    public Sprite[] cookingMethodIcons = new Sprite[System.Enum.GetValues(typeof(CookingMethod)).Length];

    public Sprite GetCookingMethodIcon(CookingMethod method)
    {
        int index = (int)method;
        if (cookingMethodIcons == null || index < 0 || index >= cookingMethodIcons.Length)
            return null;
        return cookingMethodIcons[index];
    }
}
