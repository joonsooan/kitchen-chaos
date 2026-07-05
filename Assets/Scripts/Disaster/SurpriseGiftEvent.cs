using System.Collections;
using UnityEngine;

// 깜짝 선물 — duration 동안 보상(코인·점수) 배율 적용 후 복원. (InvertControlsEvent 코루틴 패턴 재사용)
public class SurpriseGiftEvent : DisasterEvent
{
    [SerializeField] private float duration = 60f;
    [SerializeField] private int rewardMultiplier = 2;

    public override float Duration => duration;
    public override Color PopupFlashColor => SkyFlash;   // 하늘색

    protected override bool TryTrigger()
    {
        if (GameManager.Instance == null) return false;

        StopAllCoroutines();
        StartCoroutine(GiftRoutine());
        return true;
    }

    private IEnumerator GiftRoutine()
    {
        GameManager.Instance.RewardMultiplier = rewardMultiplier;   // AddReward가 코인·점수에 적용
        yield return new WaitForSeconds(duration);
        if (GameManager.Instance != null) GameManager.Instance.RewardMultiplier = 1;
    }
}
