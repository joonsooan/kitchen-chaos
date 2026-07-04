using System;
using UnityEngine;

public class GameManager : KSingleton<GameManager>
{
    public static event Action<int> OnMoneyChanged;
    public static event Action<int> OnScoreChanged;
    public static event Action<float> OnTimeTick;

    public int Money { get; private set; }
    public int Score { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsRunning { get; private set; }

    // 버프용 보상 배율 (LuckBuff가 설정) — 주문 보상에만 적용
    public int RewardMultiplier { get; set; } = 1;

    [Header("주문 보상 - 잔여 인내심 비율 보너스")]
    [Tooltip("잔여 인내심 비율이 이 값 이상이면 보너스 배율 적용")]
    [SerializeField, Range(0f, 1f)] private float bonusToleranceRatioThreshold = 0.5f;
    [Tooltip("보너스 조건 충족 시 보상에 곱해지는 배율")]
    [SerializeField] private float bonusRewardMultiplier = 1.5f;

    [Header("주문 실패 페널티")]
    [Tooltip("제한 시간 초과 또는 잘못된 요리 제공 시 차감할 점수 (양수로 입력)")]
    [SerializeField] private int orderFailurePenaltyScore = 10;

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartGame()
    {
        Money = 0;
        Score = 0;
        ElapsedTime = 0f;
        IsRunning = true;
        SoundManager.Instance?.SetPhase(GamePhase.Stage);
    }

    public void StopGame()
    {
        IsRunning = false;
    }

    private void Update()
    {
        if (!IsRunning) return;

        float prev = ElapsedTime;
        ElapsedTime += Time.deltaTime;

        int prevSec = Mathf.FloorToInt(prev);
        int currSec = Mathf.FloorToInt(ElapsedTime);
        if (currSec > prevSec)
            OnTimeTick?.Invoke(ElapsedTime);
    }

    // remainingToleranceRatio: 서빙 시점 기준 남은 인내심 비율 (0~1)
    public void AddReward(RecipeData recipe, float remainingToleranceRatio)
    {
        float bonusMultiplier = remainingToleranceRatio >= bonusToleranceRatioThreshold
            ? bonusRewardMultiplier
            : 1f;

        AddMoney(Mathf.RoundToInt(recipe.moneyReward * RewardMultiplier * bonusMultiplier));
        AddScore(Mathf.RoundToInt(recipe.scoreReward * RewardMultiplier * bonusMultiplier));
        SoundManager.Instance?.PlaySFX(SFXType.RewardGain);
    }

    public void ApplyOrderFailurePenalty()
    {
        AddScore(-orderFailurePenaltyScore);
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }
}
