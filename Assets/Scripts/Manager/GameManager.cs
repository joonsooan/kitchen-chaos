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

    public void AddReward(RecipeData recipe)
    {
        AddMoney(recipe.moneyReward * RewardMultiplier);
        AddScore(recipe.scoreReward * RewardMultiplier);
        SoundManager.Instance?.PlaySFX(SFXType.RewardGain);
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
