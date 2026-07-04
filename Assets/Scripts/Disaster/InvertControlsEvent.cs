using System.Collections;
using UnityEngine;

public class InvertControlsEvent : DisasterEvent
{
    [SerializeField] private float duration = 10f;

    public override float Duration => duration;

    protected override bool TryTrigger()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null) return false;

        StopAllCoroutines();
        StartCoroutine(InvertRoutine(player));
        return true;
    }

    private IEnumerator InvertRoutine(PlayerMovement player)
    {
        player.InputInverted = true;
        yield return new WaitForSeconds(duration);
        player.InputInverted = false;
    }
}
