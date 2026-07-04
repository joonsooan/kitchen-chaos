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
    }

    private void HandleItemDropped(CarryingItemType type)
    {
        heldItemSortingGroup = null;
    }
}
