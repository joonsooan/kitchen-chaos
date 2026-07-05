using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Table serving point. F with a dish (any non-empty container) delivers it instantly
/// to the waiting customer seated at this table. Serving judges the dish here (matching
/// the customer's order), pays the reward at once on success, fires OnDishServed for the
/// UI, then hands the container to CustomerDishReturn so the customer eats and later
/// returns it - the dish is NOT destroyed. (Judging directly instead of via
/// Customer.ReceiveRecipe, which would send the customer straight out and skip the
/// eat-and-return sequence.)
/// </summary>
[RequireComponent(typeof(Table))]
[DisallowMultipleComponent]
public class TableServing : MonoBehaviour, IInteractable
{
    private const float SeatMatchEpsilon = 0.1f;   // TeleportServeRunner와 동일하게 통일

    // Raised after a delivery is judged (success flag included) so UI (order slots etc.)
    // can react. Fires whether or not the order was correct.
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
        if (succeeded)
        {
            float tolerance = customer.CustomerData.toleranceSeconds;
            float remainingRatio = tolerance > 0f
                ? Mathf.Clamp01(customer.RemainingPatience / tolerance)
                : 0f;
            GameManager.Instance.AddReward(recipe, remainingRatio);
        }
        else
        {
            GameManager.Instance.ApplyOrderFailurePenalty();     // 잘못된 접시 = 주문 실패
            SoundManager.Instance?.PlaySFX(SFXType.OrderFailed);
        }

        OnDishServed?.Invoke(table, customer, recipe, succeeded);

        string dishName = recipe != null ? recipe.recipeName : "unfinished dish";
        Debug.Log($"[TableServing] Delivered {dishName} - {(succeeded ? "correct order!" : "wrong order...")}");

        customer.GetComponent<CustomerDishReturn>().Begin(container, succeeded);
        player.ServeDish();
    }

    // Seated customers snap exactly onto Seat.SitWorldPosition, so a position match
    // finds this table's customers without needing a seat->customer reference.
    // 우선순위: 배달된 요리를 주문한 손님이 있으면 무조건 그 손님. 후보가 여럿이면
    // (둘 다 주문 일치이거나, 아무도 일치하지 않을 때) 남은 인내심이 가장 적은 =
    // 가장 급한 손님에게 준다. 이 tie-break는 TeleportServeBuff의 "가장 급한 매칭
    // 손님" 선정과 동일하므로, 순간이동 버프가 그 손님을 지목해 Interact를 대신
    // 호출해도 여기서 같은 손님이 선택된다.
    private Customer FindWaitingCustomer(RecipeData deliveredRecipe)
    {
        IReadOnlyList<Seat> seats = table.LinkedSeats;
        if (seats == null) return null;

        Customer[] customers = FindObjectsByType<Customer>(FindObjectsSortMode.None);
        Customer bestMatch = null;      // 주문 일치 손님 중 남은 인내심이 가장 적은 손님
        Customer bestFallback = null;   // 일치 손님이 없을 때, 남은 인내심이 가장 적은 손님

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

                bool matches = deliveredRecipe != null && candidate.CustomerData != null
                    && candidate.CustomerData.requiredRecipe == deliveredRecipe;

                if (matches)
                {
                    if (bestMatch == null || candidate.RemainingPatience < bestMatch.RemainingPatience)
                        bestMatch = candidate;
                }
                else if (bestFallback == null || candidate.RemainingPatience < bestFallback.RemainingPatience)
                {
                    bestFallback = candidate;
                }
            }
        }

        return bestMatch != null ? bestMatch : bestFallback;
    }
}
