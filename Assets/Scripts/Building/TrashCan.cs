using UnityEngine;

/// <summary>Marks the trash can. Identification log only - the discard flow comes later.</summary>
[DisallowMultipleComponent]
public class TrashCan : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        Debug.Log($"[TrashCan] {name}: item={player.CurrentItemType}");
    }
}
