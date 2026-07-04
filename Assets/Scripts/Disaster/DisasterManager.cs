using System;
using System.Collections.Generic;
using UnityEngine;

public class DisasterManager : KSingleton<DisasterManager>
{
    [Serializable]
    private class BuildingTrigger
    {
        public IngredientSource source;
        public DisasterEvent disasterEvent;
    }

    [SerializeField] private float firstTriggerSeconds = 60f;
    [SerializeField] private float triggerIntervalSeconds = 60f;
    [SerializeField] private DisasterEvent[] timeTriggerPool;
    [SerializeField] private BuildingTrigger[] buildingTriggers;

    private float nextTimeTrigger;
    private float disasterEndTime = -1f;
    private readonly List<(IngredientSource source, Action<IngredientSource, PlayerController> handler)> buildingSubscriptions = new();

    private void OnEnable()
    {
        GameManager.OnTimeTick += HandleTimeTick;
        DisasterEvent.OnAnyDisasterTriggered += HandleDisasterTriggered;
        SubscribeBuildingTriggers();
    }

    private void OnDisable()
    {
        GameManager.OnTimeTick -= HandleTimeTick;
        DisasterEvent.OnAnyDisasterTriggered -= HandleDisasterTriggered;
        UnsubscribeBuildingTriggers();
    }

    private void HandleTimeTick(float elapsedTime)
    {
        // 이전 재앙 효과가 끝날 때까지 대기 — 끝나면 그 시점부터 인터벌 재계산
        if (disasterEndTime >= 0f)
        {
            if (elapsedTime < disasterEndTime) return;

            disasterEndTime = -1f;
            nextTimeTrigger = elapsedTime + triggerIntervalSeconds;
            return;
        }

        if (nextTimeTrigger <= 0f) nextTimeTrigger = firstTriggerSeconds;
        if (elapsedTime < nextTimeTrigger) return;

        DisasterEvent triggered = TriggerRandomEvent(timeTriggerPool);
        if (triggered != null) disasterEndTime = elapsedTime + triggered.Duration;
    }

    private void HandleDisasterTriggered(DisasterEvent disasterEvent)
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.ShowPopupUI<DisasterPopup>().Setup(disasterEvent.DisasterName, disasterEvent.DisasterDescription);
    }

    private void SubscribeBuildingTriggers()
    {
        if (buildingTriggers == null) return;

        foreach (BuildingTrigger trigger in buildingTriggers)
        {
            if (trigger.source == null || trigger.disasterEvent == null) continue;

            DisasterEvent disasterEvent = trigger.disasterEvent;
            Action<IngredientSource, PlayerController> handler = (source, player) => disasterEvent.Trigger();
            trigger.source.OnInteracted += handler;
            buildingSubscriptions.Add((trigger.source, handler));
        }
    }

    private void UnsubscribeBuildingTriggers()
    {
        foreach ((IngredientSource source, Action<IngredientSource, PlayerController> handler) in buildingSubscriptions)
        {
            source.OnInteracted -= handler;
        }
        buildingSubscriptions.Clear();
    }

    private DisasterEvent TriggerRandomEvent(DisasterEvent[] pool)
    {
        if (pool == null || pool.Length == 0) return null;

        DisasterEvent chosen = pool[UnityEngine.Random.Range(0, pool.Length)];
        chosen.Trigger();
        return chosen;
    }
}
