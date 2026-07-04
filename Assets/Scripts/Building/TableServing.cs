using UnityEngine;

/// <summary>
/// Marks a table as a (future) serving point. Serving needs the carried-dish identity,
/// so for now this only logs identification plus the table's linked seat count.
/// </summary>
[RequireComponent(typeof(Table))]
[DisallowMultipleComponent]
public class TableServing : MonoBehaviour, IInteractable
{
    private Table table;

    private void Awake()
    {
        table = GetComponent<Table>();
    }

    public void Interact(PlayerController player)
    {
        int seatCount = table.LinkedSeats != null ? table.LinkedSeats.Count : 0;
        Debug.Log($"[TableServing] {name}: seats={seatCount}, item={player.CurrentItemType}");
    }
}
