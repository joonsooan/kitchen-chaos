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
    [SerializeField] private int disasterCountToEnd = 5;       // N번째 재앙이 나올 차례가 되면 성공 종료
    [SerializeField] private int initialTargetScore = 50;      // 1페이즈 종료 시 요구 점수
    [SerializeField] private int targetScoreIncrement = 50;    // 페이즈마다 목표 증가량

    public static event Action<bool> OnDisasterGameOver;

    // 페이즈 = 재앙 이벤트 진행 구간, 휴식 = 재앙 종료 ~ 다음 재앙. (구 PhaseManager 대체)
    public int CurrentPhase => disasterCount;               // 0 = 첫 재앙 전
    public bool IsResting => disasterEndTime < 0f;
    public float SegmentRemaining { get; private set; }     // 현재 구간(페이즈/휴식) 남은 초

    // HUD 표시용 목표 점수 — 페이즈 중엔 이번 페이즈 종료 시 판정 목표, 휴식 중엔 다음 페이즈 목표
    public int CurrentTargetScore => IsResting
        ? initialTargetScore + disasterCount * targetScoreIncrement
        : initialTargetScore + (disasterCount - 1) * targetScoreIncrement;

    private float nextTimeTrigger;
    private float disasterEndTime = -1f;
    private int disasterCount;
    private bool gameOverTriggered;
    private readonly List<(IngredientSource source, Action<IngredientSource, PlayerController> handler)> buildingSubscriptions = new();

    private void OnEnable()
    {
        SegmentRemaining = firstTriggerSeconds;   // 첫 틱 전 HUD 표시용
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
        if (gameOverTriggered) return;

        // 페이즈(재앙 효과) 진행 중
        if (disasterEndTime >= 0f)
        {
            if (elapsedTime < disasterEndTime)
            {
                SegmentRemaining = disasterEndTime - elapsedTime;
                return;
            }

            // 페이즈 종료 → 점수 판정, 통과 시 휴식기 진입
            disasterEndTime = -1f;
            nextTimeTrigger = elapsedTime + triggerIntervalSeconds;
            SegmentRemaining = triggerIntervalSeconds;

            int score  = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            int target = initialTargetScore + (disasterCount - 1) * targetScoreIncrement;
            if (score < target)
            {
                gameOverTriggered = true;
                OnDisasterGameOver?.Invoke(false);
            }
            return;
        }

        // 휴식기 (게임 시작 ~ 첫 재앙 포함)
        if (nextTimeTrigger <= 0f) nextTimeTrigger = firstTriggerSeconds;
        SegmentRemaining = Mathf.Max(0f, nextTimeTrigger - elapsedTime);
        if (elapsedTime < nextTimeTrigger) return;

        // 마지막 재앙 차례 — 재앙 대신 성공 종료 (모든 페이즈 판정 통과)
        if (disasterCount >= disasterCountToEnd - 1)
        {
            gameOverTriggered = true;
            OnDisasterGameOver?.Invoke(true);
            return;
        }

        DisasterEvent triggered = TriggerRandomEvent(timeTriggerPool);
        if (triggered != null)
        {
            disasterEndTime = elapsedTime + triggered.Duration;
            SegmentRemaining = triggered.Duration;
        }
    }

    private void HandleDisasterTriggered(DisasterEvent disasterEvent)
    {
        if (disasterEvent.ShowsPopup && UIManager.Instance != null)
            UIManager.Instance.ShowPopupUI<DisasterPopup>().Setup(disasterEvent.DisasterName, disasterEvent.DisasterDescription, disasterEvent.PopupFlashColor);

        disasterCount++;   // 페이즈 시작 (판정은 페이즈 종료 시 HandleTimeTick에서)
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

        // 발동 실패(TryTrigger false) 시 null — 다음 틱에 재시도
        DisasterEvent chosen = pool[PickWeightedIndex(pool)];
        return chosen.Trigger() ? chosen : null;
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
