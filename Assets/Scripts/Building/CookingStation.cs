using System;
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

    public event Action<CookingStation> OnCookingStarted;
    public event Action<CookingStation, IngredientInstance> OnCookingFinished;

    private Building building;
    private IngredientPickup loadedItem;
    private bool isCooking;
    private float cookStartTime;

    public float CookProgress01 =>
        isCooking ? Mathf.Clamp01((Time.time - cookStartTime) / cookDuration) : 0f;

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
            TakeCooked(player);
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
        cookStartTime = Time.time;
        if (requiresPresence) player.ChangeState(PlayerState.Busy);
        OnCookingStarted?.Invoke(this);

        yield return new WaitForSeconds(cookDuration);

        loadedItem.Instance.ApplyCookingMethod(Method);
        OnCookingFinished?.Invoke(this, loadedItem.Instance);
        isCooking = false;
        if (requiresPresence) player.ChangeState(PlayerState.Idle);
        Debug.Log($"[CookingStation] {name}: done - {loadedItem.Instance.Data.ingredientName} is now {Method}");
    }

    private void TakeCooked(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;

        // 접시/컵을 들고 있으면 조리물을 그 그릇에 담는다 (조리법 무관).
        if (held.Type == CarryingItemType.Plate || held.Type == CarryingItemType.Cup)
        {
            ContainerKitchenObject container = held.WorldObject.GetComponent<ContainerKitchenObject>();
            if (container.TryAddIngredient(loadedItem.Instance))
            {
                Destroy(loadedItem.gameObject);
                loadedItem = null;
            }
            return;
        }

        // 빈손 → 도마(requiresPresence)의 조리물만 맨손 회수 허용, 나머지는 접시로만.
        if (held.Type == CarryingItemType.None)
        {
            if (!requiresPresence)
            {
                Debug.Log($"[CookingStation] {name}: 조리된 재료는 접시나 컵을 들고 와서 담으세요");
                return;
            }

            IngredientPickup item = loadedItem;
            loadedItem = null;
            item.transform.SetParent(null);
            item.Interact(player);
        }
    }
}
