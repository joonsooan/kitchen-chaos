using UnityEngine;

/// <summary>
/// Trash can. Destroys a held ingredient; a held container (cup/plate) only gets
/// emptied - containers are a finite resource spawned once at game start.
/// </summary>
[DisallowMultipleComponent]
public class TrashCan : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;

        switch (held.Type)
        {
            case CarryingItemType.Ingredient:
                if (held.WorldObject != null) Destroy(held.WorldObject);
                player.ClearHeldItem();
                Debug.Log("[TrashCan] Discarded ingredient");
                break;

            case CarryingItemType.Cup:
            case CarryingItemType.Plate:
                ContainerKitchenObject container = held.WorldObject != null
                    ? held.WorldObject.GetComponent<ContainerKitchenObject>()
                    : null;
                if (container == null) return;
                container.Empty();
                Debug.Log($"[TrashCan] Emptied {held.Type}");
                break;

            default:
                Debug.Log("[TrashCan] Nothing to discard");
                break;
        }
    }
}
