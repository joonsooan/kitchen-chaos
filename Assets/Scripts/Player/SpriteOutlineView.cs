using UnityEngine;

public class SpriteOutlineView
{
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

    private readonly GameObject go;
    private readonly SpriteRenderer spriteRenderer;
    private readonly float scaleMultiplier;

    public SpriteOutlineView(string name, Color color, float scaleMultiplier, string sortingLayerName, int sortingOrder)
    {
        go = new GameObject(name);
        spriteRenderer = go.AddComponent<SpriteRenderer>();

        Shader shader = Shader.Find("Custom/SpriteOutline");
        Material material = new Material(shader);
        material.SetColor(OutlineColorId, color);
        spriteRenderer.material = material;

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = sortingOrder;

        this.scaleMultiplier = scaleMultiplier;

        go.SetActive(false);
    }

    public void Show(Sprite sprite, Vector3 worldPosition, Vector3 scale)
    {
        go.SetActive(true);
        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        go.transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
        go.transform.localScale = scale;
    }

    public void Show(SpriteRenderer source)
    {
        go.SetActive(true);
        spriteRenderer.sprite = source.sprite;
        spriteRenderer.flipX = source.flipX;
        spriteRenderer.flipY = source.flipY;
        spriteRenderer.sortingLayerID = source.sortingLayerID;
        spriteRenderer.sortingOrder = source.sortingOrder - 1;
        go.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
        go.transform.localScale = source.transform.lossyScale * scaleMultiplier;
    }

    public void Hide()
    {
        if (go.activeSelf) go.SetActive(false);
    }

    public void Destroy()
    {
        Object.Destroy(go);
    }
}
