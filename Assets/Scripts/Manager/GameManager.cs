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
        AddMoney(recipe.moneyReward);
        AddScore(recipe.scoreReward);
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
