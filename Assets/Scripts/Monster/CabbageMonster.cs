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
        Destroy(gameObject);
    }
}
