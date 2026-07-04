using UnityEngine;

public class CabbageMonster : MonoBehaviour, IAttackable
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

    public void Hit(PlayerController player)
    {
        TakeDamage(PowerBuff.OneShotActive ? currentHealth : 1);   // 파워 업 버프: 한 방
    }

    private void TakeDamage(int amount)
    {
        // 타격 소리(SFXType.Hit)는 PlayerInteraction.TryAttack에서 통합 재생.
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
