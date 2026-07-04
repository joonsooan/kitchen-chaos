using UnityEngine;

/// <summary>
/// Read-only presentation layer for an ingredient item's runtime state: mirrors the
/// IngredientInstance (name + cooking state) into inspector fields and darkens the
/// sprite once cooked. The real state lives in IngredientPickup.Instance - this only
/// reflects it, so future visuals (per-state sprites etc.) belong here too.
/// </summary>
[RequireComponent(typeof(IngredientPickup))]
[DisallowMultipleComponent]
public class IngredientState : MonoBehaviour
{
    [SerializeField] private string ingredientName;
    [SerializeField] private CookingMethod currentState;

    private IngredientPickup pickup;
    private SpriteRenderer spriteRenderer;
    private Sprite rawSprite;

    private void Awake()
    {
        pickup = GetComponent<IngredientPickup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) rawSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        IngredientInstance instance = pickup.Instance;
        if (instance == null) return;

        if (instance.Data != null && ingredientName != instance.Data.ingredientName)
        {
            ingredientName = instance.Data.ingredientName;
        }

        if (currentState == instance.CurrentState) return;
        currentState = instance.CurrentState;
        ApplyState(instance.Data);
    }

    private void ApplyState(Ingredient data)
    {
        if (spriteRenderer == null) return;

        if (currentState == CookingMethod.None)
        {
            spriteRenderer.sprite = rawSprite;
            return;
        }

        Sprite cookedIcon = data != null ? data.GetCookingMethodIcon(currentState) : null;
        spriteRenderer.sprite = cookedIcon != null ? cookedIcon : rawSprite;
    }
}
