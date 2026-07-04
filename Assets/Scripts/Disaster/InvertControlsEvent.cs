using System.Collections;
using UnityEngine;

public class InvertControlsEvent : DisasterEvent
{
    [SerializeField] private float duration = 10f;

    public override void Trigger()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null) return;

        StopAllCoroutines();
        StartCoroutine(InvertRoutine(player));
    }

    private IEnumerator InvertRoutine(PlayerMovement player)
    {
        player.InputInverted = true;
        yield return new WaitForSeconds(duration);
        player.InputInverted = false;
    }
}
