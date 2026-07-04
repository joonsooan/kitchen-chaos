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
}
