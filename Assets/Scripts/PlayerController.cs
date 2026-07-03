using System;
using UnityEngine;

public enum PlayerState
{
    // 필요 시 State 추가 예정
    Idle,
    Moving,
    Chopping,
    Cooking,
    Fetching,
}

public enum CarryingItemType
{
    // 재료 이외에 추가로 들고 다닐 아이템 생기면 State 추가 예정
    None,
    Ingredient,
}

public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerState> OnStateChanged;
    public static event Action<CarryingItemType> OnItemPickedUp;
    public static event Action<CarryingItemType> OnItemDropped;

    public static event Action OnDishServed;
    public static event Action OnChopProgressed;
    public static event Action OnChopCompleted;

    private PlayerState currentState = PlayerState.Idle;
    private CarryingItemType currentItemType = CarryingItemType.None;

    public PlayerState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                currentState = value;
                OnStateChanged?.Invoke(currentState);
            }
        }
    }

    public CarryingItemType CurrentItemType
    {
        get => currentItemType;
        private set => currentItemType = value;
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState = newState;
    }

    public void PickUpItem(CarryingItemType itemType)
    {
        CurrentItemType = itemType;
        OnItemPickedUp?.Invoke(itemType);
    }

    public void DropItem()
    {
        CarryingItemType droppedType = CurrentItemType;
        CurrentItemType = CarryingItemType.None;
        OnItemDropped?.Invoke(droppedType);
    }

    public void ServeDish()
    {
        CurrentItemType = CarryingItemType.None;
        OnDishServed?.Invoke();
    }

    public void TriggerChopProgress()
    {
        OnChopProgressed?.Invoke();
    }

    public void TriggerChopComplete()
    {
        OnChopCompleted?.Invoke();
    }
}