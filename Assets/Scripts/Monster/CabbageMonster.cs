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
        SoundManager.Instance?.PlaySFX(SFXType.Hit);
        // TODO: 양상추 말소리 시스템 생기면 여기서 SoundManager.Instance?.PlaySFX(SFXType.LettuceVoice);
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
