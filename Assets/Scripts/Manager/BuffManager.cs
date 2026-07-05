using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 활성 버프 수명 관리 — Apply/Remove 호출과 타이머만 담당.
// UI·연출은 OnBuffStarted/OnBuffEnded 구독 + GetRemaining 폴링으로 연결 (직접 참조 없음).
public class BuffManager : KSingleton<BuffManager>
{
    // static — 매니저급 UI가 전역 1회 구독 (GameManager 이벤트와 동일 패턴)
    public static event Action<BuffData> OnBuffStarted;
    public static event Action<BuffData> OnBuffEnded;

    private readonly Dictionary<BuffData, Coroutine> active   = new();
    private readonly Dictionary<BuffData, float>     endTimes = new();

    public bool IsActive(BuffData buff) => buff != null && active.ContainsKey(buff);

    // 남은 시간(초) — 비활성이면 0 (게이지/타이머 UI 폴링용)
    public float GetRemaining(BuffData buff)
    {
        if (buff == null || !endTimes.TryGetValue(buff, out float end)) return 0f;
        return Mathf.Max(0f, end - Time.time);
    }

    public void Activate(BuffData buff)
    {
        if (buff == null || buff.duration <= 0f) return;   // 꽝 등 즉시 소멸

        // 같은 버프 재획득 → 타이머만 리셋 (Apply 중복 방지)
        if (active.TryGetValue(buff, out var running))
        {
            StopCoroutine(running);
        }
        else
        {
            buff.Apply();
        }

        endTimes[buff] = Time.time + buff.duration;
        active[buff]   = StartCoroutine(ExpireAfter(buff));
        OnBuffStarted?.Invoke(buff);
    }

    // 버프 즉시 해제 — 리롤 교체 등 (만료와 동일한 정리 경로)
    public void Deactivate(BuffData buff)
    {
        if (buff == null || !active.TryGetValue(buff, out var running)) return;

        StopCoroutine(running);
        active.Remove(buff);
        endTimes.Remove(buff);
        buff.Remove();
        OnBuffEnded?.Invoke(buff);
    }

    private IEnumerator ExpireAfter(BuffData buff)
    {
        yield return new WaitForSeconds(buff.duration);

        active.Remove(buff);
        endTimes.Remove(buff);
        buff.Remove();
        OnBuffEnded?.Invoke(buff);
    }
}
