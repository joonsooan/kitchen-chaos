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
        HeldItem held = player.CurrentHeldItem;

        // 접시/컵을 들고 있으면 이 재료를 그 그릇에 담는다 (재료 들고 접시 F 와 대칭).
        if (held.Type == CarryingItemType.Plate || held.Type == CarryingItemType.Cup)
        {
            ContainerKitchenObject container = held.WorldObject.GetComponent<ContainerKitchenObject>();
            if (container.TryAddIngredient(instance))
                Destroy(gameObject);
            return;
        }

        // 빈손이면 집는다. 바닥 재료는 생재료 또는 도마조리물뿐이라 조리상태 제한 불필요.
        if (held.Type == CarryingItemType.None)
        {
            PrepareForHold();
            player.PickUpIngredient(instance, gameObject);
        }
    }
}
