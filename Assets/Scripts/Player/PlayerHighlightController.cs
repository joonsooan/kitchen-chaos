using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInteraction))]
public class PlayerHighlightController : MonoBehaviour
{
    [SerializeField] private Color objectHighlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float outlineScale = 1.2f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int highlightSortingOrder = 500;

    private PlayerMovement movement;
    private PlayerInteraction interaction;

    private SpriteOutlineView objectHighlight;

    private void Awake()
    {
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
        UpdateObjectHighlight(movement.FacingDirection);
    }

    private void UpdateObjectHighlight(Vector2 facing)
    {
        if (!interaction.TryPeekTarget(facing, out Transform hitTransform))
        {
            objectHighlight.Hide();
            return;
        }

        // 상호작용 대상(IInteractable) 또는 타격 대상(IAttackable) 어느 쪽이든 루트를 잡아 외곽선 표시.
        Component target = hitTransform.GetComponentInParent<IInteractable>() as Component
                           ?? hitTransform.GetComponentInParent<IAttackable>() as Component;
        if (target == null)
        {
            objectHighlight.Hide();
            return;
        }

        SpriteRenderer sourceRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (sourceRenderer == null || sourceRenderer.sprite == null)
        {
            objectHighlight.Hide();
            return;
        }

        objectHighlight.Show(sourceRenderer);
    }
}
