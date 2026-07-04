using System.Collections.Generic;

public static class RecipeMatcher
{
    public static bool TryMatch(List<IngredientInstance> contents, RecipeData[] recipes, out RecipeData matched)
    {
        for (int i = 0; i < recipes.Length; i++)
        {
            if (Matches(contents, recipes[i]))
            {
                matched = recipes[i];
                return true;
            }
        }

        matched = null;
        return false;
    }

    private static bool Matches(List<IngredientInstance> contents, RecipeData recipe)
    {
        IngredientEntry[] entries = recipe.ingredients;

        int requiredTotal = 0;
        for (int i = 0; i < entries.Length; i++) requiredTotal += entries[i].amount;
        if (contents.Count != requiredTotal) return false;

        for (int i = 0; i < entries.Length; i++)
        {
            IngredientEntry entry = entries[i];
            int count = 0;
            for (int c = 0; c < contents.Count; c++)
            {
                if (contents[c].Data == entry.ingredientType && contents[c].CurrentState == entry.requiredCookingMethod)
                {
                    count++;
                }
            }

            if (count != entry.amount) return false;
        }

        return true;
    }

    // 내용물로 아직 완성 가능한(부분 multiset) 레시피가 하나라도 있으면 true.
    public static bool CanEventuallyMatch(List<IngredientInstance> contents, RecipeData[] recipes)
    {
        for (int i = 0; i < recipes.Length; i++)
            if (IsViablePrefix(contents, recipes[i])) return true;
        return false;
    }

    // 내용물이 recipe의 (재료·조리) 요구 multiset의 부분집합이면 true (아직 완성 가능).
    private static bool IsViablePrefix(List<IngredientInstance> contents, RecipeData recipe)
    {
        IngredientEntry[] entries = recipe.ingredients;
        int covered = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            IngredientEntry entry = entries[i];
            int count = 0;
            for (int c = 0; c < contents.Count; c++)
                if (contents[c].Data == entry.ingredientType && contents[c].CurrentState == entry.requiredCookingMethod)
                    count++;
            if (count > entry.amount) return false; // 이 (재료·조리) 초과 → 이 레시피 불가
            covered += count;
        }
        return covered == contents.Count; // 레시피에 없는 재료가 남으면 false
    }
}
