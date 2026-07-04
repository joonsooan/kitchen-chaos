using System.Collections.Generic;
using UnityEngine;

public class ContainerKitchenObject : GridPlaceable, IInteractable
{
    [SerializeField] private CarryingItemType containerType = CarryingItemType.Plate;
    [SerializeField] private RecipeData[] availableRecipes;

    private readonly List<IngredientInstance> contents = new List<IngredientInstance>();

    public CarryingItemType ContainerType => containerType;
    public IReadOnlyList<IngredientInstance> Contents => contents;
    public RecipeData CompletedRecipe { get; private set; }
    public bool HasCompletedDish => CompletedRecipe != null;

    public void Interact(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;

        if (held.Type == CarryingItemType.Ingredient)
        {
            if (HasCompletedDish)
            {
                Debug.Log($"이미 완성된 요리가 있습니다. ({CompletedRecipe.recipeName})");
                return;
            }

            AddIngredient(held.Ingredient);
            if (held.WorldObject != null) Destroy(held.WorldObject);
            player.ClearHeldItem();
            return;
        }

        if (held.Type == CarryingItemType.None)
        {
            PrepareForHold();
            player.PickUpContainer(containerType, contents, CompletedRecipe, gameObject);
        }
    }

    private void AddIngredient(IngredientInstance ingredient)
    {
        contents.Add(ingredient);

        if (RecipeMatcher.TryMatch(contents, availableRecipes, out RecipeData matched))
        {
            CompletedRecipe = matched;
            contents.Clear();
        }
    }
}
