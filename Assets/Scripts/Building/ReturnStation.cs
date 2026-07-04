using System.Collections;
using UnityEngine;

/// <summary>
/// Cup/plate dispenser (CupReturn/PlateReturn) with a finite stock: dispensing takes
/// one out, a customer dropping off their finished dish (AcceptReturn) puts one back,
/// so the total in circulation never exceeds initialStock. With empty hands, F spawns
/// this station's container and hands it straight to the player; full hands or Busy
/// dispense nothing.
/// </summary>
[DisallowMultipleComponent]
public class ReturnStation : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private int initialStock = 2;

    // 재고 소진 상태에서 꺼내기 시도 — UI가 구독해 경고 표시
    public static event System.Action<ReturnStation> OnDispenseFailedEmpty;

    private bool pendingPickup;
    private int currentStock;

    public CarryingItemType ContainerType => containerPrefab.GetComponent<ContainerKitchenObject>().ContainerType;
    public int CurrentStock => currentStock;   // UI 표시용 (읽기 전용)

    private void Awake()
    {
        currentStock = initialStock;
    }

    // Restock: a customer finished eating and dropped their dish back off here.
    public void AcceptReturn()
    {
        currentStock++;
        SoundManager.Instance?.PlaySFX(SFXType.Serve);   // 손님이 다 먹고 접시 반납(퇴식구 제출)
    }

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

        if (currentStock <= 0)
        {
            Debug.Log($"[ReturnStation] {name}: out of stock - wait for a customer to return one");
            OnDispenseFailedEmpty?.Invoke(this);
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
        currentStock--;
        GameObject spawned = Instantiate(containerPrefab, transform.position, Quaternion.identity);
        yield return null;
        pendingPickup = false;

        ContainerKitchenObject container = spawned.GetComponent<ContainerKitchenObject>();
        if (container != null) container.Interact(player);
    }
}
