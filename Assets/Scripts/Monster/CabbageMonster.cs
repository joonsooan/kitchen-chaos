using UnityEngine;

public class CabbageMonster : MonoBehaviour, IInteractable
{
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Init(float duration)
    {
        if (duration > 0f) Invoke(nameof(Despawn), duration);
    }

    public void Interact(PlayerController player)
    {
        TakeDamage(1);
    }

    private void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth > 0) return;

        Despawn();
    }

    private void Despawn()
    {
        CancelInvoke(nameof(Despawn));
        Destroy(gameObject);
    }
}
