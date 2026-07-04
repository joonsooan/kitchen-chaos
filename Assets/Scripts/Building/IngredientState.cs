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
    [SerializeField, Range(0f, 1f)] private float cookedDarken = 0.6f;

    private IngredientPickup pickup;
    private SpriteRenderer spriteRenderer;
    private Color rawColor;

    private void Awake()
    {
        pickup = GetComponent<IngredientPickup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) rawColor = spriteRenderer.color;
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
        ApplyStateColor();
    }

    private void ApplyStateColor()
    {
        if (spriteRenderer == null) return;

        if (currentState == CookingMethod.None)
        {
            spriteRenderer.color = rawColor;
            return;
        }

        spriteRenderer.color = new Color(
            rawColor.r * cookedDarken,
            rawColor.g * cookedDarken,
            rawColor.b * cookedDarken,
            rawColor.a);
    }
}
