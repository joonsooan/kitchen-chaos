using UnityEngine;

[CreateAssetMenu(fileName = "RecipeData", menuName = "KitchenChaos/RecipeData")]
public class RecipeData : ScriptableObject
{
    public Sprite recipeIcon;
    public string recipeName;
    public IngredientEntry[] ingredients;
    // TODO: 조리 방법 관련 필드 추가
}
