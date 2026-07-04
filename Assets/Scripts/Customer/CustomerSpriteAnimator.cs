using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CustomerMovement))]
public class CustomerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private SpriteRenderer spriteRenderer;
    private CustomerMovement customerMovement;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        customerMovement = GetComponent<CustomerMovement>();
    }

    private void LateUpdate()
    {
        Vector2 facing = customerMovement.FacingDirection;

        Sprite targetSprite = Mathf.Abs(facing.x) >= Mathf.Abs(facing.y)
            ? (facing.x >= 0f ? rightSprite : leftSprite)
            : (facing.y >= 0f ? upSprite : downSprite);

        if (targetSprite != null) spriteRenderer.sprite = targetSprite;
    }
}
