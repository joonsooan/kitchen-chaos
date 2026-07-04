using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Table serving point. F with a dish (any non-empty container) delivers it instantly
/// to the waiting customer seated at this table: the customer judges it against their
/// order - wrong or unfinished dishes fail (no reward) - and the container is consumed
/// either way. Wires the delivery -> reward chain (Customer.ReceiveRecipe then
/// GameManager.AddReward on success).
/// </summary>
[RequireComponent(typeof(Table))]
[DisallowMultipleComponent]
public class TableServing : MonoBehaviour, IInteractable
{
    private const float SeatMatchEpsilon = 0.05f;

    public static event Action<Table, Customer, RecipeData, bool> OnDishServed;

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

        // CompletedRecipe may be null (unfinished/wrong mix) - the customer's own
        // comparison then fails it, which is exactly the intended judgment.
        RecipeData recipe = container.CompletedRecipe;

        Customer customer = FindWaitingCustomer(recipe);
        if (customer == null)
        {
            Debug.Log($"[TableServing] {name}: no waiting customer at this table");
            return;
        }
        bool succeeded = false;
        Action<Customer, RecipeData> markSucceeded = (c, r) => succeeded = true;

        customer.OnOrderSucceeded += markSucceeded;
        customer.ReceiveRecipe(recipe);
        customer.OnOrderSucceeded -= markSucceeded;

        if (succeeded) GameManager.Instance.AddReward(recipe);
        OnDishServed?.Invoke(table, customer, recipe, succeeded);
        string dishName = recipe != null ? recipe.recipeName : "unfinished dish";
        Debug.Log($"[TableServing] Delivered {dishName} - {(succeeded ? "correct order!" : "wrong order...")}");

        Destroy(held.WorldObject);
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
