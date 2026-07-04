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
    [SerializeField] private float[] timeTriggerWeights;   // timeTriggerPool과 1:1. 비었거나 짧으면 가중치 1.
    [SerializeField] private BuildingTrigger[] buildingTriggers;
    [SerializeField] private bool infiniteDisasters;
    [SerializeField] private int disasterCountToEnd = 5;
    [SerializeField] private int minScoreToSucceed = 0;

    public static event Action<bool> OnDisasterGameOver;

    private float nextTimeTrigger;
    private float disasterEndTime = -1f;
    private int disasterCount;
    private bool gameOverTriggered;
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
        if (disasterEvent.ShowsPopup && UIManager.Instance != null)
            UIManager.Instance.ShowPopupUI<DisasterPopup>().Setup(disasterEvent.DisasterName, disasterEvent.DisasterDescription, disasterEvent.PopupFlashColor);

        if (infiniteDisasters || gameOverTriggered) return;

        disasterCount++;
        if (disasterCount < disasterCountToEnd) return;

        gameOverTriggered = true;
        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        OnDisasterGameOver?.Invoke(score >= minScoreToSucceed);
    }

    private void SubscribeBuildingTriggers()
    {
        if (buildingTriggers == null) return;

        foreach (BuildingTrigger trigger in buildingTriggers)
        {
            if (trigger.source == null || trigger.disasterEvent == null) continue;

            DisasterEvent disasterEvent = trigger.disasterEvent;
            Action<IngredientSource, PlayerController> handler = (source, player) => disasterEvent.OnBuildingInteract(player);
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

        DisasterEvent chosen = pool[PickWeightedIndex(pool)];
        chosen.Trigger();
        return chosen;
    }

    // 가중 랜덤 (CustomerSpawner와 동일 패턴). 매번 합산 → 합이 얼마든 자동 정규화.
    private float GetWeight(int index)
    {
        if (timeTriggerWeights != null && index < timeTriggerWeights.Length) return Mathf.Max(0f, timeTriggerWeights[index]);
        return 1f;
    }

    private int PickWeightedIndex(DisasterEvent[] pool)
    {
        float total = 0f;
        for (int i = 0; i < pool.Length; i++) total += GetWeight(i);
        if (total <= 0f) return UnityEngine.Random.Range(0, pool.Length);

        float r = UnityEngine.Random.value * total;
        for (int i = 0; i < pool.Length; i++)
        {
            r -= GetWeight(i);
            if (r < 0f) return i;
        }
        return pool.Length - 1;
    }
}
