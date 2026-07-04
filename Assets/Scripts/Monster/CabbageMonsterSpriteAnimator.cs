using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CabbageMonsterMovement))]
public class CabbageMonsterSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private SpriteRenderer spriteRenderer;
    private CabbageMonsterMovement movement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<CabbageMonsterMovement>();
    }

    private void LateUpdate()
    {
        Vector2 facing = movement.FacingDirection;

        Sprite targetSprite = Mathf.Abs(facing.x) >= Mathf.Abs(facing.y)
            ? (facing.x >= 0f ? rightSprite : leftSprite)
            : (facing.y >= 0f ? upSprite : downSprite);

        if (targetSprite != null) spriteRenderer.sprite = targetSprite;
    }
}
