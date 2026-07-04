using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void LateUpdate()
    {
        Vector2 facing = playerMovement.FacingDirection;

        Sprite targetSprite = Mathf.Abs(facing.x) >= Mathf.Abs(facing.y)
            ? (facing.x >= 0f ? rightSprite : leftSprite)
            : (facing.y >= 0f ? upSprite : downSprite);

        if (targetSprite != null) spriteRenderer.sprite = targetSprite;
    }
}
