using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 활성 버프 수명 관리 — Apply/Remove 호출과 타이머만 담당.
// UI·연출은 OnBuffStarted/OnBuffEnded 구독으로 연결 (직접 참조 없음).
public class BuffManager : KSingleton<BuffManager>
{
    public event Action<BuffData> OnBuffStarted;
    public event Action<BuffData> OnBuffEnded;

    private readonly Dictionary<BuffData, Coroutine> active = new();

    public bool IsActive(BuffData buff) => buff != null && active.ContainsKey(buff);

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

        active[buff] = StartCoroutine(ExpireAfter(buff));
        OnBuffStarted?.Invoke(buff);
    }

    private IEnumerator ExpireAfter(BuffData buff)
    {
        yield return new WaitForSeconds(buff.duration);

        active.Remove(buff);
        buff.Remove();
        OnBuffEnded?.Invoke(buff);
    }
}
