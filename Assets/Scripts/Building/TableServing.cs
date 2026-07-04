using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Table serving point. F with a dish (any non-empty container) delivers it instantly
/// to the waiting customer seated at this table. Serving judges the dish here (matching
/// the customer's order) and pays the reward at once on success, then hands the container
/// to CustomerDishReturn so the customer eats and later returns it - the dish is NOT
/// destroyed. (This bypasses Customer.ReceiveRecipe, which would send the customer
/// straight out and skip the eat-and-return sequence.)
/// </summary>
[RequireComponent(typeof(Table))]
[DisallowMultipleComponent]
public class TableServing : MonoBehaviour, IInteractable
{
    private const float SeatMatchEpsilon = 0.05f;

    private Table table;

    private void Awake()
    {
        table = GetComponent<Table>();
    }

    public void Interact(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;
        if (held.Type != CarryingItemType.Cup && held.Type != CarryingItemType.Plate)
        {
            Debug.Log($"[TableServing] {name}: bring a dish (item={held.Type})");
            return;
        }

        ContainerKitchenObject container = held.WorldObject != null
            ? held.WorldObject.GetComponent<ContainerKitchenObject>()
            : null;
        if (container == null) return;

        // An untouched container is not a dish - block it so a stray F near the
        // table doesn't burn the customer and the container.
        if (!container.HasCompletedDish && container.Contents.Count == 0)
        {
            Debug.Log($"[TableServing] {name}: the {held.Type} is empty - nothing to serve");
            return;
        }

        // CompletedRecipe may be null (unfinished/wrong mix) - the judgment below
        // then fails it, which is exactly the intended outcome.
        RecipeData recipe = container.CompletedRecipe;

        Customer customer = FindWaitingCustomer(recipe);
        if (customer == null)
        {
            Debug.Log($"[TableServing] {name}: no waiting customer at this table");
            return;
        }
        // Same judgment as Customer.ReceiveRecipe, done here so we can pay the reward
        // up front and still route the dish into the eat-and-return sequence.
        bool succeeded = recipe != null && customer.CustomerData.requiredRecipe == recipe;
        if (succeeded) GameManager.Instance.AddReward(recipe);

        string dishName = recipe != null ? recipe.recipeName : "unfinished dish";
        Debug.Log($"[TableServing] Delivered {dishName} - {(succeeded ? "correct order!" : "wrong order...")}");

        customer.GetComponent<CustomerDishReturn>().Begin(container, succeeded);
        player.ServeDish();
    }

    // Seated customers snap exactly onto Seat.SitWorldPosition, so a position match
    // finds this table's customers without needing a seat->customer reference.
    // Prefers the customer whose order matches the delivered recipe, so with two
    // seated customers the dish never lands on the wrong one by seat order.
    private Customer FindWaitingCustomer(RecipeData deliveredRecipe)
    {
        IReadOnlyList<Seat> seats = table.LinkedSeats;
        if (seats == null) return null;

        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        Customer fallback = null;

        for (int s = 0; s < seats.Count; s++)
        {
            Seat seat = seats[s];
            if (seat == null || !seat.IsOccupied) continue;

            Vector3 sitPosition = seat.SitWorldPosition;
            for (int c = 0; c < customers.Length; c++)
            {
                Customer candidate = customers[c];
                if (candidate.CurrentState != CustomerState.Waiting) continue;

                Vector3 delta = candidate.transform.position - sitPosition;
                delta.z = 0f;
                if (delta.sqrMagnitude > SeatMatchEpsilon * SeatMatchEpsilon) continue;

                if (deliveredRecipe != null && candidate.CustomerData != null
                    && candidate.CustomerData.requiredRecipe == deliveredRecipe)
                {
                    return candidate;
                }

                if (fallback == null) fallback = candidate;
            }
        }

        return fallback;
    }
}
