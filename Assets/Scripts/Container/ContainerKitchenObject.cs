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
    [SerializeField] private float stackYOffset = 0.1f;
    [SerializeField] private float stackXOffset = 0.05f;
    [SerializeField] private float stackRotationRange = 10f;

    private readonly List<IngredientInstance> contents = new List<IngredientInstance>();
    private readonly List<GameObject> ingredientVisuals = new List<GameObject>();
    private SpriteRenderer spriteRenderer;

    public CarryingItemType ContainerType => containerType;
    public IReadOnlyList<IngredientInstance> Contents => contents;
    public RecipeData CompletedRecipe { get; private set; }
    public bool HasCompletedDish => CompletedRecipe != null;
    public ContainerState State { get; private set; } = ContainerState.InProgress;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

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
        SpawnIngredientVisual(GetIngredientVisualSprite(ingredient));

        if (RecipeMatcher.TryMatch(contents, availableRecipes, out RecipeData matched))
        {
            State = ContainerState.Complete;
            CompletedRecipe = matched;
            contents.Clear();
            ClearIngredientVisuals();
            SpawnIngredientVisual(matched.recipeIcon);
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        // 완성은 아님 — 아직 완성 가능하면 미완성, 어떤 레시피도 불가능하면 실패.
        State = RecipeMatcher.CanEventuallyMatch(contents, availableRecipes)
            ? ContainerState.InProgress
            : ContainerState.Failed;
    }

    private static Sprite GetIngredientVisualSprite(IngredientInstance ingredient)
    {
        if (ingredient.Data == null) return null;

        Sprite cookedIcon = ingredient.Data.GetCookingMethodIcon(ingredient.CurrentState);
        return cookedIcon != null ? cookedIcon : ingredient.Data.ingredientIcon;
    }

    private void SpawnIngredientVisual(Sprite sprite)
    {
        if (sprite == null) return;

        int stackIndex = ingredientVisuals.Count;

        Transform anchor = ingredientVisualAnchor != null ? ingredientVisualAnchor : transform;
        GameObject visual = new GameObject(sprite.name);
        visual.transform.SetParent(anchor, false);
        visual.transform.localPosition = new Vector3(Random.Range(-stackXOffset, stackXOffset), stackYOffset * stackIndex, 0f);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-stackRotationRange, stackRotationRange));
        visual.transform.localScale = Vector3.one * visualScale;

        SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder + stackIndex;

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
        State = ContainerState.InProgress;
        ClearIngredientVisuals();
        if (spriteRenderer != null) spriteRenderer.enabled = true;
    }
}
