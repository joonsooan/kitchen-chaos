using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Moving,
    Busy,
}

public enum CarryingItemType
{
    // 재료 이외에 추가로 들고 다닐 아이템 생기면 State 추가 예정
    None,
    Ingredient,
    Plate,
    Cup,
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
    private HeldItem currentHeldItem = HeldItem.None;
    private Collider2D col;

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

    // 손엔 항상 HeldItem 1개만 존재
    public HeldItem CurrentHeldItem => currentHeldItem;
    public CarryingItemType CurrentItemType => currentHeldItem.Type;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    public void ChangeState(PlayerState newState)
    {
        CurrentState = newState;
    }

    public void SetColliderEnabled(bool enabled)
    {
        if (col != null) col.enabled = enabled;
    }

    public void PickUpIngredient(IngredientInstance ingredient, GameObject worldObject)
    {
        currentHeldItem = HeldItem.OfIngredient(ingredient, worldObject);
        OnItemPickedUp?.Invoke(currentHeldItem.Type);
    }

    public void PickUpContainer(CarryingItemType containerType, List<IngredientInstance> contents, RecipeData completedRecipe, GameObject worldObject)
    {
        currentHeldItem = HeldItem.OfContainer(containerType, new List<IngredientInstance>(contents), completedRecipe, worldObject);
        OnItemPickedUp?.Invoke(currentHeldItem.Type);
    }

    public void ClearHeldItem()
    {
        CarryingItemType droppedType = currentHeldItem.Type;
        currentHeldItem = HeldItem.None;
        OnItemDropped?.Invoke(droppedType);
    }

    public void ServeDish()
    {
        currentHeldItem = HeldItem.None;
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