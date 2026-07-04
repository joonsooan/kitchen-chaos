using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHeldItemVisual : MonoBehaviour
{
    [SerializeField] private Transform heldItemAnchor;
    [SerializeField] private float anchorDistance = 0.6f;

    private PlayerController controller;
    private PlayerMovement movement;
    private SpriteRenderer playerSpriteRenderer;
    private SortingGroup heldItemSortingGroup;
    private ContainerKitchenObject heldContainer;
    private SpriteRenderer heldContainerSpriteRenderer;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        PlayerController.OnItemPickedUp += HandleItemPickedUp;
        PlayerController.OnItemDropped += HandleItemDropped;
    }

    private void OnDisable()
    {
        PlayerController.OnItemPickedUp -= HandleItemPickedUp;
        PlayerController.OnItemDropped -= HandleItemDropped;
    }

    private void LateUpdate()
    {
        if (heldItemAnchor == null) return;
        heldItemAnchor.localPosition = movement.FacingDirection.normalized * anchorDistance;

        // 들고 있는 동안 조리대에서 완성될 수도 있으므로 매 프레임 상태를 동기화한다.
        if (heldContainerSpriteRenderer != null)
        {
            heldContainerSpriteRenderer.enabled = !heldContainer.HasCompletedDish;
        }

        if (heldItemSortingGroup == null) return;

        // 위쪽/좌우 스프라이트일 때는 뒤쪽(플레이어보다 낮은 order), 아래쪽일 때만 앞쪽으로 렌더링
        Vector2 facing = movement.FacingDirection;
        bool facingDown = Mathf.Abs(facing.x) < Mathf.Abs(facing.y) && facing.y < 0f;

        heldItemSortingGroup.sortingLayerID = playerSpriteRenderer.sortingLayerID;
        heldItemSortingGroup.sortingOrder = facingDown
            ? playerSpriteRenderer.sortingOrder + 1
            : playerSpriteRenderer.sortingOrder - 1;
    }

    private void HandleItemPickedUp(CarryingItemType type)
    {
        if (heldItemAnchor == null) return;

        GameObject worldObject = controller.CurrentHeldItem.WorldObject;
        if (worldObject == null) return;

        worldObject.transform.SetParent(heldItemAnchor, false);
        worldObject.transform.localPosition = Vector3.zero;

        heldItemSortingGroup = worldObject.GetComponent<SortingGroup>();
        if (heldItemSortingGroup == null) heldItemSortingGroup = worldObject.AddComponent<SortingGroup>();

        // 완성된 요리는 레시피 아이콘이 그릇 스프라이트를 대체하므로 원래 그릇 스프라이트는 숨긴다.
        // 픽업 후 조리대에서 완성될 수도 있어 LateUpdate에서 매 프레임 재확인한다.
        heldContainer = worldObject.GetComponent<ContainerKitchenObject>();
        heldContainerSpriteRenderer = heldContainer != null ? worldObject.GetComponent<SpriteRenderer>() : null;
        if (heldContainerSpriteRenderer != null)
        {
            heldContainerSpriteRenderer.enabled = !heldContainer.HasCompletedDish;
        }
    }

    private void HandleItemDropped(CarryingItemType type)
    {
        heldItemSortingGroup = null;

        if (heldContainerSpriteRenderer != null)
        {
            heldContainerSpriteRenderer.enabled = true;
        }
        heldContainer = null;
        heldContainerSpriteRenderer = null;
    }
}
