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

    // 주문 UI(InGameHUD)가 손님 스폰과 무관하게 주문 등록/제거 시점만 알면 되도록 static 이벤트로 노출
    public static event Action<Customer> OnAnyCustomerSeated;
    public static event Action<Customer> OnAnyCustomerLeft;

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

    // 주문 데이터 주입 (레시피·tolerance)
    public void SetData(CustomerData data)
    {
        customerData = data;
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
        SoundManager.Instance?.PlaySFX(SFXType.OrderCreated);
        OnAnyCustomerSeated?.Invoke(this);
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
            GameManager.Instance?.ApplyOrderFailurePenalty();
            SoundManager.Instance?.PlaySFX(SFXType.OrderFailed);
            OnOrderFailed?.Invoke(this, customerData.requiredRecipe);
        }

        OnAnyCustomerLeft?.Invoke(this);
    }
}
