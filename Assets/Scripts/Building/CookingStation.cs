using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Deposit-based cooking station. F with a raw ingredient loads it into the station
/// and starts a timer; F again with empty hands retrieves the cooked ingredient.
/// requiresPresence stations (cutting board) lock the player (Busy) for the duration;
/// the rest (fry pan, mixer) cook unattended.
/// </summary>
[RequireComponent(typeof(Building))]
[DisallowMultipleComponent]
public class CookingStation : MonoBehaviour, IInteractable
{
    [SerializeField] private float cookDuration = 5f;
    [SerializeField] private bool requiresPresence;

    [Header("Mix Visual (Mixer only)")]
    [SerializeField] private SpriteRenderer visualSpriteRenderer;
    [SerializeField] private Ingredient mixIngredientA;
    [SerializeField] private Sprite mixingSpriteA;
    [SerializeField] private Ingredient mixIngredientB;
    [SerializeField] private Sprite mixingSpriteB;
    [SerializeField] private Ingredient mixIngredientC;
    [SerializeField] private Sprite mixingSpriteC;
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private float shakeSpeed = 0.05f;

    [Header("Cooking Particles")]
    [SerializeField] private ParticleSystem cookingParticles;

    public event Action<CookingStation> OnCookingStarted;
    public event Action<CookingStation, IngredientInstance> OnCookingFinished;

    private Building building;
    private IngredientPickup loadedItem;
    private bool isCooking;
    private float cookStartTime;
    private Sprite defaultSprite;
    private Vector3 defaultVisualLocalPosition;
    private Tween shakeTween;

    public float CookProgress01 =>
        isCooking ? Mathf.Clamp01((Time.time - cookStartTime) / cookDuration) : 0f;

    private CookingMethod Method =>
        building.BuildingData != null ? building.BuildingData.cookingMethod : CookingMethod.None;

    private void Awake()
    {
        if (cookingParticles != null) cookingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        building = GetComponent<Building>();
        if (visualSpriteRenderer != null)
        {
            defaultSprite = visualSpriteRenderer.sprite;
            defaultVisualLocalPosition = visualSpriteRenderer.transform.localPosition;
        }
    }

    public void Interact(PlayerController player)
    {
        if (isCooking)
        {
            Debug.Log($"[CookingStation] {name}: still cooking...");
            return;
        }

        if (loadedItem != null)
        {
            TakeCooked(player);
            return;
        }

        TryLoad(player);
    }

    private void TryLoad(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;
        if (held.Type != CarryingItemType.Ingredient || held.WorldObject == null)
        {
            Debug.Log($"[CookingStation] {name}: empty (method={Method}) - bring a raw ingredient");
            return;
        }

        if (held.Ingredient == null || held.Ingredient.CurrentState != CookingMethod.None)
        {
            Debug.Log($"[CookingStation] {name}: that ingredient is already cooked - no re-cooking");
            return;
        }

        IngredientPickup pickup = held.WorldObject.GetComponent<IngredientPickup>();
        if (pickup == null)
        {
            Debug.LogWarning($"[CookingStation] {name}: held object has no IngredientPickup");
            return;
        }

        loadedItem = pickup;
        Transform itemTransform = pickup.transform;
        itemTransform.SetParent(transform, true);
        itemTransform.position = transform.position;
        player.ClearHeldItem();

        // 믹서는 자체 mix visual로 표현하므로 재료 스프라이트는 숨긴다.
        if (Method == CookingMethod.Mix)
        {
            SpriteRenderer itemRenderer = pickup.GetComponent<SpriteRenderer>();
            if (itemRenderer != null) itemRenderer.enabled = false;
        }

        StartCoroutine(Cook(player));
    }

    private IEnumerator Cook(PlayerController player)
    {
        isCooking = true;
        cookStartTime = Time.time;
        if (requiresPresence)
        {
            player.ChangeState(PlayerState.Busy);
            player.SetColliderEnabled(false);
            player.SetInputEnabled(false);
        }
        OnCookingStarted?.Invoke(this);

        // 조리 시작 효과음 — 스테이션별(도마/후라이펜/믹서기)
        switch (Method)
        {
            case CookingMethod.Chop: SoundManager.Instance?.PlaySFX(SFXType.Chop); break;
            case CookingMethod.Fry: SoundManager.Instance?.PlaySFX(SFXType.Fry); break;
            case CookingMethod.Mix: SoundManager.Instance?.PlaySFX(SFXType.Mix); break;
        }

        if (Method == CookingMethod.Mix)
        {
            StartMixVisual(loadedItem.Instance.Data);
        }

        if (cookingParticles != null) cookingParticles.Play(true);

        yield return new WaitForSeconds(cookDuration);

        if (cookingParticles != null) cookingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (Method == CookingMethod.Mix)
        {
            StopMixVisual();
        }

        loadedItem.Instance.ApplyCookingMethod(Method);
        OnCookingFinished?.Invoke(this, loadedItem.Instance);
        isCooking = false;
        if (requiresPresence)
        {
            player.SetColliderEnabled(true);
            player.SetInputEnabled(true);
            player.ChangeState(PlayerState.Idle);
        }
        Debug.Log($"[CookingStation] {name}: done - {loadedItem.Instance.Data.ingredientName} is now {Method}");
    }

    private Sprite GetMixingSprite(Ingredient ingredient)
    {
        if (ingredient == mixIngredientA) return mixingSpriteA;
        if (ingredient == mixIngredientB) return mixingSpriteB;
        if (ingredient == mixIngredientC) return mixingSpriteC;
        return null;
    }

    private void StartMixVisual(Ingredient ingredient)
    {
        if (visualSpriteRenderer == null) return;

        Sprite mixingSprite = GetMixingSprite(ingredient);
        if (mixingSprite != null)
        {
            visualSpriteRenderer.sprite = mixingSprite;
        }

        shakeTween?.Kill();
        Vector3 pos = defaultVisualLocalPosition;
        pos.x -= shakeStrength;
        visualSpriteRenderer.transform.localPosition = pos;
        shakeTween = visualSpriteRenderer.transform
            .DOLocalMoveX(defaultVisualLocalPosition.x + shakeStrength, shakeSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    // 흔들림/위치만 정리. 믹서 스프라이트는 재료를 비울 때(ResetMixVisual)까지 유지한다.
    private void StopMixVisual()
    {
        if (visualSpriteRenderer == null) return;

        shakeTween?.Kill();
        shakeTween = null;
        visualSpriteRenderer.transform.localPosition = defaultVisualLocalPosition;
    }

    private void ResetMixVisual()
    {
        if (visualSpriteRenderer == null) return;

        visualSpriteRenderer.sprite = defaultSprite;
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }

    private void TakeCooked(PlayerController player)
    {
        HeldItem held = player.CurrentHeldItem;

        // 접시/컵을 들고 있으면 조리물을 그 그릇에 담는다 (조리법 무관).
        if (held.Type == CarryingItemType.Plate || held.Type == CarryingItemType.Cup)
        {
            ContainerKitchenObject container = held.WorldObject.GetComponent<ContainerKitchenObject>();
            if (container.TryAddIngredient(loadedItem.Instance))
            {
                Destroy(loadedItem.gameObject);
                loadedItem = null;
                if (Method == CookingMethod.Mix) ResetMixVisual();
            }
            return;
        }

        // 빈손 → 도마(requiresPresence)의 조리물만 맨손 회수 허용, 나머지는 접시로만.
        if (held.Type == CarryingItemType.None)
        {
            if (!requiresPresence)
            {
                Debug.Log($"[CookingStation] {name}: 조리된 재료는 접시나 컵을 들고 와서 담으세요");
                return;
            }

            IngredientPickup item = loadedItem;
            loadedItem = null;
            if (Method == CookingMethod.Mix) ResetMixVisual();
            item.transform.SetParent(null);
            item.Interact(player);
        }
    }
}
