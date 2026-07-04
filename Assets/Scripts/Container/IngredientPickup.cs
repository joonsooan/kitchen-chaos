using UnityEngine;

public class IngredientPickup : GridPlaceable, IInteractable
{
    [SerializeField] private Ingredient ingredientData;
    [SerializeField] private CookingMethod initialState = CookingMethod.None;

    private IngredientInstance instance;

    public IngredientInstance Instance => instance;

    protected override void Awake()
    {
        base.Awake();
        instance = new IngredientInstance(ingredientData, initialState);
    }

    public void Interact(PlayerController player)
    {
        if (player.CurrentHeldItem.Type != CarryingItemType.None) return;

        PrepareForHold();
        player.PickUpIngredient(instance, gameObject);
    }
}
