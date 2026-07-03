using UnityEngine;

/// <summary>Test-only component to verify the raycast interaction system end-to-end.</summary>
public class DebugInteractable : MonoBehaviour, IInteractable
{
    private static readonly Color[] CycleColors = { Color.green, Color.yellow, Color.cyan };

    public int InteractCount { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Interact(PlayerController player)
    {
        InteractCount++;

        if (spriteRenderer != null) spriteRenderer.color = CycleColors[InteractCount % CycleColors.Length];

        Debug.Log($"[DebugInteractable] {name} interacted x{InteractCount} (state={player.CurrentState}, item={player.CurrentItemType})");
    }
}
