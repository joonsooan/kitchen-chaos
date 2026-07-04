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
    private readonly List<(IngredientSource source, Action<IngredientSource, PlayerController> handler)> buildingSubscriptions = new();

    private void OnEnable()
    {
        GameManager.OnTimeTick += HandleTimeTick;
        SubscribeBuildingTriggers();
    }

    private void OnDisable()
    {
        GameManager.OnTimeTick -= HandleTimeTick;
        UnsubscribeBuildingTriggers();
    }

    private void HandleTimeTick(float elapsedTime)
    {
        if (nextTimeTrigger <= 0f) nextTimeTrigger = firstTriggerSeconds;
        if (elapsedTime < nextTimeTrigger) return;

        nextTimeTrigger += triggerIntervalSeconds;
        TriggerRandomEvent(timeTriggerPool);
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

    private void TriggerRandomEvent(DisasterEvent[] pool)
    {
        if (pool == null || pool.Length == 0) return;

        pool[UnityEngine.Random.Range(0, pool.Length)].Trigger();
    }
}
