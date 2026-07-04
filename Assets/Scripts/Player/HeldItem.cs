using System.Collections.Generic;
using UnityEngine;

public class HeldItem
{
    public static readonly HeldItem None = new HeldItem(CarryingItemType.None, null, null, null, null);

    public CarryingItemType Type { get; }
    public IngredientInstance Ingredient { get; }
    public List<IngredientInstance> Contents { get; }
    public RecipeData CompletedRecipe { get; }
    public GameObject WorldObject { get; }

    private HeldItem(CarryingItemType type, IngredientInstance ingredient, List<IngredientInstance> contents, RecipeData completedRecipe, GameObject worldObject)
    {
        Type = type;
        Ingredient = ingredient;
        Contents = contents;
        CompletedRecipe = completedRecipe;
        WorldObject = worldObject;
    }

    public static HeldItem OfIngredient(IngredientInstance ingredient, GameObject worldObject) =>
        new HeldItem(CarryingItemType.Ingredient, ingredient, null, null, worldObject);

    public static HeldItem OfContainer(CarryingItemType containerType, List<IngredientInstance> contents, RecipeData completedRecipe, GameObject worldObject) =>
        new HeldItem(containerType, null, contents ?? new List<IngredientInstance>(), completedRecipe, worldObject);
}
