using UnityEngine;

/// <summary>
/// Marks a cooking appliance (fry pan, cutting board, mixer). Identification only for
/// now - logs which cooking method this station provides. The real cooking flow starts
/// once carried-ingredient identity is designed.
/// </summary>
[RequireComponent(typeof(Building))]
[DisallowMultipleComponent]
public class CookingStation : MonoBehaviour, IInteractable
{
    private Building building;

    private void Awake()
    {
        building = GetComponent<Building>();
    }

    public void Interact(PlayerController player)
    {
        CookingMethod method = building.BuildingData != null
            ? building.BuildingData.cookingMethod
            : CookingMethod.None;
        Debug.Log($"[CookingStation] {name}: method={method}, item={player.CurrentItemType}");
    }
}
