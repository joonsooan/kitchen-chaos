using System;
using UnityEngine;
using UnityEngine.Pool;

public enum CustomerState
{
    Idle,
    Moving,
    Waiting,
    LeavingSuccess,
    LeavingFailure,
}

public class Customer : MonoBehaviour
{
    public event Action<CustomerState> OnStateChanged;
    public event Action<Customer, RecipeData> OnOrderSucceeded;
    public event Action<Customer, RecipeData> OnOrderFailed;

    [SerializeField] private CustomerData customerData;

    private CustomerState currentState = CustomerState.Idle;
    private float waitTimer;
    private IObjectPool<GameObject> pool;

    public CustomerData CustomerData => customerData;
    public float RemainingPatience => waitTimer;

    public CustomerState CurrentState
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

    public void ChangeState(CustomerState newState)
    {
        CurrentState = newState;
    }

    public void SetPool(IObjectPool<GameObject> ownerPool)
    {
        pool = ownerPool;
    }

    public void ReturnToPool()
    {
        if (pool != null) pool.Release(gameObject);
        else Destroy(gameObject);
    }

    public void Seat()
    {
        CurrentState = CustomerState.Waiting;
        waitTimer = customerData.toleranceSeconds;
    }

    public void ReceiveRecipe(RecipeData deliveredRecipe)
    {
        if (CurrentState != CustomerState.Waiting) return;

        bool isCorrect = deliveredRecipe == customerData.requiredRecipe;
        Leave(isCorrect);
    }

    private void Update()
    {
        if (CurrentState != CustomerState.Waiting) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            Leave(false);
        }
    }

    private void Leave(bool success)
    {
        CurrentState = success ? CustomerState.LeavingSuccess : CustomerState.LeavingFailure;

        if (success)
        {
            OnOrderSucceeded?.Invoke(this, customerData.requiredRecipe);
        }
        else
        {
            OnOrderFailed?.Invoke(this, customerData.requiredRecipe);
        }
    }
}
