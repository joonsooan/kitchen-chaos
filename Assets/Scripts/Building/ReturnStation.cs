using System.Collections;
using UnityEngine;

/// <summary>
/// Cup/plate dispenser (CupReturn/PlateReturn). With empty hands, F spawns this
/// station's container and hands it straight to the player; full hands or Busy
/// dispense nothing.
/// </summary>
[DisallowMultipleComponent]
public class ReturnStation : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject containerPrefab;

    private bool pendingPickup;

    public void Interact(PlayerController player)
    {
        if (pendingPickup) return;

        if (player.CurrentItemType != CarryingItemType.None || player.CurrentState == PlayerState.Busy)
        {
            Debug.Log($"[ReturnStation] {name}: hands full or busy - nothing dispensed");
            return;
        }

        if (containerPrefab == null)
        {
            Debug.LogWarning($"[ReturnStation] {name}: containerPrefab unassigned");
            return;
        }

        StartCoroutine(SpawnAndPickUp(player));
    }

    // Same one-frame delay as IngredientSource: GridPlaceable.Start auto-places a
    // fresh spawn on its current cell next frame, which would yank a same-frame
    // pickup back out of the player's hands.
    private IEnumerator SpawnAndPickUp(PlayerController player)
    {
        pendingPickup = true;
        GameObject spawned = Instantiate(containerPrefab, transform.position, Quaternion.identity);
        yield return null;
        pendingPickup = false;

        ContainerKitchenObject container = spawned.GetComponent<ContainerKitchenObject>();
        if (container != null) container.Interact(player);
    }
}
