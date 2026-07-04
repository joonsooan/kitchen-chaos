using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
public class PlayerHighlightController : MonoBehaviour
{
    [SerializeField] private Color objectHighlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float outlineScale = 1.2f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int highlightSortingOrder = 500;

    private PlayerController controller;
    private PlayerMovement movement;
    private PlayerInteraction interaction;

    private SpriteOutlineView objectHighlight;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
        interaction = GetComponent<PlayerInteraction>();

        objectHighlight = new SpriteOutlineView("ObjectHighlight", objectHighlightColor, outlineScale, sortingLayerName, highlightSortingOrder);
    }

    private void OnDestroy()
    {
        objectHighlight.Destroy();
    }

    private void LateUpdate()
    {
        Vector2 facing = movement.FacingDirection;

        if (controller.CurrentHeldItem.Type == CarryingItemType.None)
        {
            UpdateObjectHighlight(facing);
        }
        else
        {
            objectHighlight.Hide();
        }
    }

    private void UpdateObjectHighlight(Vector2 facing)
    {
        if (!interaction.TryPeekInteractable(facing, out Transform hitTransform))
        {
            objectHighlight.Hide();
            return;
        }

        GridPlaceable placeable = hitTransform.GetComponentInParent<GridPlaceable>();
        if (placeable == null)
        {
            objectHighlight.Hide();
            return;
        }

        SpriteRenderer sourceRenderer = placeable.GetComponentInChildren<SpriteRenderer>();
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            objectHighlight.Hide();
            return;
        }

        objectHighlight.Show(sourceRenderer);
    }
}
