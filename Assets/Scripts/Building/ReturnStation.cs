using UnityEngine;

/// <summary>
/// Shared marker for the cup/plate return points (distinguished by object name).
/// Their gameplay semantics are not decided yet - identification log only.
/// </summary>
[DisallowMultipleComponent]
public class ReturnStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        Debug.Log($"[ReturnStation] {name}: semantics TBD, item={player.CurrentItemType}");
    }
}
