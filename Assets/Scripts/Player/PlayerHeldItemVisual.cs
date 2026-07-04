using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerHeldItemVisual : MonoBehaviour
{
    [SerializeField] private Transform heldItemAnchor;
    [SerializeField] private float anchorDistance = 0.6f;

    private PlayerController controller;
    private PlayerMovement movement;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        PlayerController.OnItemPickedUp += HandleItemPickedUp;
    }

    private void OnDisable()
    {
        PlayerController.OnItemPickedUp -= HandleItemPickedUp;
    }

    private void LateUpdate()
    {
        if (heldItemAnchor == null) return;
        heldItemAnchor.localPosition = movement.FacingDirection.normalized * anchorDistance;
    }

    private void HandleItemPickedUp(CarryingItemType type)
    {
        if (heldItemAnchor == null) return;

        GameObject worldObject = controller.CurrentHeldItem.WorldObject;
        if (worldObject == null) return;

        worldObject.transform.SetParent(heldItemAnchor, false);
        worldObject.transform.localPosition = Vector3.zero;
    }
}
