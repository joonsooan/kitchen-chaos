using System;

[Serializable]
public struct IngredientEntry
{
    public Ingredient ingredientType;
    public CookingMethod requiredCookingMethod;
    public int amount;
}
