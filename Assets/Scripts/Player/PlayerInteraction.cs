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
        movement.AttackPressed += HandleAttackPressed;
    }

    private void OnDisable()
    {
        movement.InteractPressed -= HandleInteractPressed;
        movement.AttackPressed -= HandleAttackPressed;
    }

    // F: 상호작용 대상 우선 → 없으면 들고 있는 아이템 내려놓기
    private void HandleInteractPressed()
    {
        if (TryInteract(movement.FacingDirection)) return;
        TryPlaceHeldItem();
    }

    // 마우스 좌클릭: 타격 (양배추 괴물·잡초)
    private void HandleAttackPressed()
    {
        TryAttack(movement.FacingDirection);
    }

    /// <summary>
    /// Raycasts from the player toward direction looking for the nearest IInteractable.
    /// Public with a direction parameter so tests/AI can drive it directly.
    /// </summary>
    public bool TryInteract(Vector2 direction)
    {
        if (!TryFindInDirection(direction, out IInteractable interactable, out _)) return false;

        interactable.Interact(controller);
        return true;
    }

    /// <summary>
    /// Raycasts toward direction for the nearest IAttackable (양배추 괴물·잡초) and hits it.
    /// 타격 소리는 여기서 통합 재생(히트 성공 시 1회).
    /// </summary>
    public bool TryAttack(Vector2 direction)
    {
        if (!TryFindInDirection(direction, out IAttackable attackable, out _)) return false;

        SoundManager.Instance?.PlaySFX(SFXType.Hit);
        attackable.Hit(controller);
        return true;
    }

    // 하이라이트용: 상호작용 대상(IInteractable) 또는 타격 대상(IAttackable) 중 가장 가까운 것을 반환.
    public bool TryPeekTarget(Vector2 direction, out Transform hitTransform)
    {
        int count = Physics2D.Raycast(transform.position, direction.normalized, contactFilter, hitBuffer, interactDistance);

        for (int i = 0; i < count; i++)
        {
            Transform candidateTransform = hitBuffer[i].collider.transform;
            if (candidateTransform == transform || candidateTransform.IsChildOf(transform)) continue;

            if (hitBuffer[i].collider.GetComponentInParent<IInteractable>() != null ||
                hitBuffer[i].collider.GetComponentInParent<IAttackable>() != null)
            {
                hitTransform = candidateTransform;
                return true;
            }
        }

        hitTransform = null;
        return false;
    }

    private bool TryFindInDirection<T>(Vector2 direction, out T found, out Transform hitTransform) where T : class
    {
        int count = Physics2D.Raycast(transform.position, direction.normalized, contactFilter, hitBuffer, interactDistance);

        for (int i = 0; i < count; i++)
        {
            Transform candidateTransform = hitBuffer[i].collider.transform;
            if (candidateTransform == transform || candidateTransform.IsChildOf(transform)) continue;

            T candidate = hitBuffer[i].collider.GetComponentInParent<T>();
            if (candidate != null)
            {
                found = candidate;
                hitTransform = candidateTransform;
                return true;
            }
        }

        found = null;
        hitTransform = null;
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
