using UnityEngine;

public class CabbageMonster : MonoBehaviour, IAttackable, IHasHealth
{
    [SerializeField] private int maxHealth = 3;

    [Header("양상추 말소리")]
    [SerializeField] private float voiceIntervalMin = 1f;
    [SerializeField] private float voiceIntervalMax = 4f;

    private int currentHealth;
    private bool hpBarShown;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // 인스턴스별 독립 랜덤 말소리 — WaitForSeconds(min~max)로 최소 간격 보장. 파괴 시 자동 종료.
    private System.Collections.IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(voiceIntervalMin, voiceIntervalMax));
            SoundManager.Instance?.PlaySFX(SFXType.LettuceVoice);
        }
    }

    public void Init(float duration)
    {
        if (duration > 0f) Invoke(nameof(Despawn), duration);
    }

    public void Hit(PlayerController player)
    {
        if (!hpBarShown)   // 첫 피격 시에만 HP 바 노출 (스폰 시엔 숨김)
        {
            MonsterHPBarView.Show(this);
            hpBarShown = true;
        }
        TakeDamage(PowerBuff.OneShotActive ? currentHealth : 1);   // 파워 업 버프: 한 방
    }

    private void TakeDamage(int amount)
    {
        // 타격 소리(SFXType.Hit)는 PlayerInteraction.TryAttack에서 통합 재생. (말소리는 Start 코루틴에서 랜덤 재생)
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
