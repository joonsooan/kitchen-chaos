using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerController))]
[DisallowMultipleComponent]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactDistance = 1.2f;

    private PlayerMovement movement;
    private PlayerController controller;

    private ContactFilter2D contactFilter;
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[8];

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();

        contactFilter.useTriggers = true;
        contactFilter.useLayerMask = false;
    }

    private void OnEnable()
    {
        movement.InteractPressed += HandleInteractPressed;
    }

    private void OnDisable()
    {
        movement.InteractPressed -= HandleInteractPressed;
    }

    private void HandleInteractPressed()
    {
        if (TryInteract(movement.FacingDirection)) return;
        TryPlaceHeldItem();
    }

    /// <summary>
    /// Raycasts from the player toward direction looking for the nearest IInteractable.
    /// Public with a direction parameter so tests/AI can drive it directly.
    /// </summary>
    public bool TryInteract(Vector2 direction)
    {
        int count = Physics2D.Raycast(transform.position, direction.normalized, contactFilter, hitBuffer, interactDistance);

        for (int i = 0; i < count; i++)
        {
            Transform hitTransform = hitBuffer[i].collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;

            IInteractable interactable = hitBuffer[i].collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(controller);
                return true;
            }
        }

        return false;
    }

    private void TryPlaceHeldItem()
    {
        HeldItem held = controller.CurrentHeldItem;
        if (held.Type == CarryingItemType.None || held.WorldObject == null) return;

        GridSystem grid = GridSystem.Instance;
        Vector2Int targetCell = grid.GetFacingCell(transform.position, movement.FacingDirection);
        if (!grid.CanPlaceOnCell(targetCell)) return;

        GridPlaceable placeable = held.WorldObject.GetComponent<GridPlaceable>();
        if (placeable == null || !placeable.PlaceAt(targetCell)) return;

        controller.ClearHeldItem();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = Application.isPlaying ? movement.FacingDirection : Vector2.down;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * interactDistance));
    }
}
