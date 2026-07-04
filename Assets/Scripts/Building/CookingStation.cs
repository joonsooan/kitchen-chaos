using System.Collections;
using UnityEngine;

/// <summary>
/// Deposit-based cooking station. F with a raw ingredient loads it into the station
/// and starts a timer; F again with empty hands retrieves the cooked ingredient.
/// requiresPresence stations (cutting board) lock the player (Busy) for the duration;
/// the rest (fry pan, mixer) cook unattended.
/// </summary>
[RequireComponent(typeof(Building))]
[DisallowMultipleComponent]
public class CookingStation : MonoBehaviour, IInteractable
{
    [SerializeField] private float cookDuration = 5f;
    [SerializeField] private bool requiresPresence;

    private Building building;
    private IngredientPickup loadedItem;
    private bool isCooking;

    private CookingMethod Method =>
        building.BuildingData != null ? building.BuildingData.cookingMethod : CookingMethod.None;

    private void Awake()
    {
        building = GetComponent<Building>();
    }

    public void Interact(PlayerController player)
    {
        if (isCooking)
        {
            Debug.Log($"[CookingStation] {name}: still cooking...");
            return;
        }

        if (loadedItem != null)
        {
            RetrieveCooked(player);
            return;
        }

        TryLoad(player);
    }

    private void TryLoad(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;
        if (held.Type != CarryingItemType.Ingredient || held.WorldObject == null)
        {
            Debug.Log($"[CookingStation] {name}: empty (method={Method}) - bring a raw ingredient");
            return;
        }

        if (held.Ingredient == null || held.Ingredient.CurrentState != CookingMethod.None)
        {
            Debug.Log($"[CookingStation] {name}: that ingredient is already cooked - no re-cooking");
            return;
        }

        IngredientPickup pickup = held.WorldObject.GetComponent<IngredientPickup>();
        if (pickup == null)
        {
            Debug.LogWarning($"[CookingStation] {name}: held object has no IngredientPickup");
            return;
        }

        loadedItem = pickup;
        Transform itemTransform = pickup.transform;
        itemTransform.SetParent(transform, true);
        itemTransform.position = transform.position;
        player.ClearHeldItem();

        StartCoroutine(Cook(player));
    }

    private IEnumerator Cook(PlayerController player)
    {
        isCooking = true;
        if (requiresPresence) player.ChangeState(PlayerState.Busy);

        yield return new WaitForSeconds(cookDuration);

        loadedItem.Instance.ApplyCookingMethod(Method);
        isCooking = false;
        if (requiresPresence) player.ChangeState(PlayerState.Idle);
        Debug.Log($"[CookingStation] {name}: done - {loadedItem.Instance.Data.ingredientName} is now {Method}");
    }

    private void RetrieveCooked(PlayerController player)
    {
        if (player.CurrentItemType != CarryingItemType.None)
        {
            Debug.Log($"[CookingStation] {name}: empty your hands to take the cooked ingredient");
            return;
        }

        IngredientPickup item = loadedItem;
        loadedItem = null;
        item.transform.SetParent(null);
        item.Interact(player);
    }
}
