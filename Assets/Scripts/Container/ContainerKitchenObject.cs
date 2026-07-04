using System.Collections.Generic;
using UnityEngine;

public class ContainerKitchenObject : GridPlaceable, IInteractable
{
    [SerializeField] private CarryingItemType containerType = CarryingItemType.Plate;
    [SerializeField] private RecipeData[] availableRecipes;

    [Header("Ingredient Visual")]
    [SerializeField] private Transform ingredientVisualAnchor;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder;
    [SerializeField] private float visualScale = 0.5f;

    private readonly List<IngredientInstance> contents = new List<IngredientInstance>();
    private readonly List<GameObject> ingredientVisuals = new List<GameObject>();

    public CarryingItemType ContainerType => containerType;
    public IReadOnlyList<IngredientInstance> Contents => contents;
    public RecipeData CompletedRecipe { get; private set; }
    public bool HasCompletedDish => CompletedRecipe != null;

    public void Interact(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;

        if (held.Type == CarryingItemType.Ingredient)
        {
            if (TryAddIngredient(held.Ingredient))
            {
                if (held.WorldObject != null) Destroy(held.WorldObject);
                player.ClearHeldItem();
            }
            return;
        }

        if (held.Type == CarryingItemType.None)
        {
            PrepareForHold();
            player.PickUpContainer(containerType, contents, CompletedRecipe, gameObject);
        }
    }

    // 재료 픽업·조리대 등 외부에서 든 그릇에 담을 때도 쓰는 공용 진입점.
    public bool TryAddIngredient(IngredientInstance ingredient)
    {
        if (HasCompletedDish)
        {
            Debug.Log($"이미 완성된 요리가 있습니다. ({CompletedRecipe.recipeName})");
            return false;
        }

        AddIngredient(ingredient);
        return true;
    }

    private void AddIngredient(IngredientInstance ingredient)
    {
        contents.Add(ingredient);
        SpawnIngredientVisual(ingredient.Data != null ? ingredient.Data.ingredientIcon : null);

        if (RecipeMatcher.TryMatch(contents, availableRecipes, out RecipeData matched))
        {
            CompletedRecipe = matched;
            contents.Clear();
            ClearIngredientVisuals();
            SpawnIngredientVisual(matched.recipeIcon);
        }
    }

    private void SpawnIngredientVisual(Sprite sprite)
    {
        if (sprite == null) return;

        Transform anchor = ingredientVisualAnchor != null ? ingredientVisualAnchor : transform;
        GameObject visual = new GameObject(sprite.name);
        visual.transform.SetParent(anchor, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * visualScale;

        SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;

        ingredientVisuals.Add(visual);
    }

    private void ClearIngredientVisuals()
    {
        for (int i = 0; i < ingredientVisuals.Count; i++)
        {
            if (ingredientVisuals[i] != null) Destroy(ingredientVisuals[i]);
        }
        ingredientVisuals.Clear();
    }

    public void Empty()
    {
        contents.Clear();
        CompletedRecipe = null;
        ClearIngredientVisuals();
    }
}
