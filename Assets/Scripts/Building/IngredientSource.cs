using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Ingredient basket. With empty hands, F spawns this basket's ingredient item and
/// hands it straight to the player; full hands or Busy dispense nothing.
/// </summary>
[DisallowMultipleComponent]
public class IngredientSource : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject itemPrefab;

    private bool pendingPickup;

    public event Action<IngredientSource, PlayerController> OnInteracted;

    public void Interact(PlayerController player)
    {
        OnInteracted?.Invoke(this, player);

        if (pendingPickup) return;

        if (player.CurrentItemType != CarryingItemType.None || player.CurrentState == PlayerState.Busy)
        {
            Debug.Log($"[IngredientSource] {name}: hands full or busy - nothing dispensed");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning($"[IngredientSource] {name}: itemPrefab unassigned");
            return;
        }

        StartCoroutine(SpawnAndPickUp(player));
    }

    // GridPlaceable.Start auto-places a fresh spawn on its current cell one frame after
    // Instantiate, which would yank a same-frame pickup back out of the player's hands.
    // Let that frame pass (it parks on the basket cell, harmless), then hand it over.
    private IEnumerator SpawnAndPickUp(PlayerController player)
    {
        pendingPickup = true;
        GameObject item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        yield return null;
        pendingPickup = false;

        IngredientPickup pickup = item.GetComponent<IngredientPickup>();
        if (pickup != null) pickup.Interact(player);
    }
}
