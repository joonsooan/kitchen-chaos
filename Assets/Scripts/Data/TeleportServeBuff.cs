using System.Collections.Generic;
using UnityEngine;

// 순간 이동 마법 — 기간 동안 완성된 요리를 들면 가장 급한(남은 인내심 최소) 매칭 주문에 자동 서빙.
// 해당 손님이 앉은 테이블의 TableServing.Interact를 대신 호출 → 판정·보상·이벤트 전부 기존 파이프라인 그대로.
[CreateAssetMenu(fileName = "TeleportServeBuff", menuName = "KitchenChaos/Buff/TeleportServe")]
public class TeleportServeBuff : BuffData
{
    private TeleportServeRunner runner;

    public override void Apply()
    {
        if (runner != null) return;
        runner = new GameObject("TeleportServeRunner").AddComponent<TeleportServeRunner>();
    }

    public override void Remove()
    {
        if (runner != null) Object.Destroy(runner.gameObject);
        runner = null;
    }
}

// 버프 기간 동안만 존재하는 폴링 러너
public class TeleportServeRunner : MonoBehaviour
{
    private const float PollInterval = 0.25f;
    private const float SeatMatchEpsilon = 0.1f;

    private float nextPoll;

    private void Update()
    {
        if (Time.time < nextPoll) return;
        nextPoll = Time.time + PollInterval;

        var player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        HeldItem held = player.CurrentHeldItem;
        if (held.Type != CarryingItemType.Cup && held.Type != CarryingItemType.Plate) return;

        var container = held.WorldObject != null
            ? held.WorldObject.GetComponent<ContainerKitchenObject>()
            : null;
        if (container == null || !container.HasCompletedDish) return;

        RecipeData recipe = container.CompletedRecipe;
        if (recipe == null) return;

        // 가장 급한(남은 인내심 최소) 매칭 주문 손님
        Customer best = null;
        float bestRemain = float.MaxValue;
        foreach (var customer in FindObjectsByType<Customer>(FindObjectsSortMode.None))
        {
            if (customer.CurrentState != CustomerState.Waiting) continue;
            if (customer.CustomerData == null || customer.CustomerData.requiredRecipe != recipe) continue;

            if (customer.RemainingPatience < bestRemain)
            {
                bestRemain = customer.RemainingPatience;
                best = customer;
            }
        }
        if (best == null) return;

        // 그 손님이 앉은 테이블의 TableServing에 대리 서빙
        foreach (var serving in FindObjectsByType<TableServing>(FindObjectsSortMode.None))
        {
            if (!SeatsContain(serving, best)) continue;

            serving.Interact(player);
            return;
        }
    }

    private static bool SeatsContain(TableServing serving, Customer customer)
    {
        var table = serving.GetComponent<Table>();
        IReadOnlyList<Seat> seats = table != null ? table.LinkedSeats : null;
        if (seats == null) return false;

        for (int i = 0; i < seats.Count; i++)
        {
            if (seats[i] == null || !seats[i].IsOccupied) continue;

            Vector3 delta = customer.transform.position - seats[i].SitWorldPosition;
            delta.z = 0f;
            if (delta.sqrMagnitude <= SeatMatchEpsilon * SeatMatchEpsilon) return true;
        }
        return false;
    }
}
