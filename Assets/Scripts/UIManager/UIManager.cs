using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : KSingleton<UIManager>
{
    // Addressables 전환 시: new AddressablesProvider() 한 줄 교체
    IAssetProvider _loader = new ResourcesProvider();

    Stack<UIPopup>         _popupStack  = new Stack<UIPopup>();
    Stack<UIPopup>         _systemStack = new Stack<UIPopup>();
    Dictionary<Type,UIHUD> _hudCache    = new Dictionary<Type,UIHUD>();

    // PauseGameWhileOpen 팝업 개수 — 중첩 시에도 마지막 하나가 닫힐 때만 재개
    int   _pauseRequestCount;
    float _cachedTimeScale = 1f;

    // ── Roots ─────────────────────────────────────────────────────────
    public GameObject HUDRoot    => GetOrCreateRoot("@UI_HUDRoot",    UIType.HUD);
    public GameObject PopupRoot  => GetOrCreateRoot("@UI_PopupRoot",  UIType.Popup);
    public GameObject SystemRoot => GetOrCreateRoot("@UI_SystemRoot", UIType.System);

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }

    GameObject GetOrCreateRoot(string name, UIType type)
    {
        // Unity fake-null 대응: ?? 대신 명시적 null 체크
        var root = GameObject.Find(name);
        if (root == null) root = new GameObject(name);

        if (root.GetComponent<Canvas>() == null)
            root.AddComponent<Canvas>();
        if (root.GetComponent<CanvasScaler>() == null)
            root.AddComponent<CanvasScaler>();
        if (root.GetComponent<GraphicRaycaster>() == null)
            root.AddComponent<GraphicRaycaster>();

        // AddComponent 완료 후 fresh ref — stale ref 방지
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = UITypes.GetSortingOrder(type);

        // 모니터/해상도 무관하게 동일 비율 유지 — Scale With Screen Size
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 1f;
        return root;
    }

    // ── HUD (캐시 — 한 번만 인스턴스화, Show/Hide로 재사용) ────────────
    public T ShowHUDUI<T>(string name = null) where T : UIHUD
    {
        // Unity fake-null: 씬 전환으로 캐시된 HUD가 파괴됐으면 캐시 미스로 보고 재인스턴스화
        if (_hudCache.TryGetValue(typeof(T), out var cached) && cached != null)
        {
            cached.gameObject.SetActive(true);
            return (T)cached;
        }

        name ??= typeof(T).Name;
        var go = Instantiate(_loader.Load($"UI/HUD/{name}"));
        var ui = go.GetComponent<T>();
        if (ui == null) ui = go.AddComponent<T>();
        go.transform.SetParent(HUDRoot.transform, false);
        ui.Init();
        _hudCache[typeof(T)] = ui;
        return ui;
    }

    public void HideHUDUI<T>() where T : UIHUD
    {
        if (_hudCache.TryGetValue(typeof(T), out var c) && c != null)
            c.gameObject.SetActive(false);
    }

    // ── Popup (스택 — LIFO, 맨 위만 활성) ────────────────────────────
    public T ShowPopupUI<T>(string name = null) where T : UIPopup
    {
        name ??= typeof(T).Name;
        var go = Instantiate(_loader.Load($"UI/Popup/{name}"));
        var popup = go.GetComponent<T>();
        if (popup == null) popup = go.AddComponent<T>();
        _popupStack.Push(popup);
        go.transform.SetParent(PopupRoot.transform, false);
        go.transform.SetAsLastSibling();
        popup.Init();
        if (popup.PauseGameWhileOpen) RequestPause();
        SoundManager.Instance?.PlaySFX(SFXType.PopupOpen);
        return popup;
    }

    public void ClosePopupUI(UIPopup popup)
    {
        if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
            ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0) return;

        var popup = _popupStack.Pop();
        if (popup.PauseGameWhileOpen) ReleasePause();

        // 닫힘 연출 후 파괴 (연출 중 스택엔 이미 없음)
        popup.PlayHideFx(() => { if (popup != null) Destroy(popup.gameObject); });
    }

    void RequestPause()
    {
        if (_pauseRequestCount == 0) _cachedTimeScale = Time.timeScale;
        _pauseRequestCount++;
        Time.timeScale = 0f;
    }

    void ReleasePause()
    {
        if (_pauseRequestCount == 0) return;

        _pauseRequestCount--;
        if (_pauseRequestCount == 0) Time.timeScale = _cachedTimeScale;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    // ── System (항상 최상단 — 연결 끊김·에러·로딩) ────────────────────
    public T ShowSystemUI<T>(string name = null) where T : UIPopup
    {
        name ??= typeof(T).Name;
        var go = Instantiate(_loader.Load($"UI/System/{name}"));
        var popup = go.GetComponent<T>();
        if (popup == null) popup = go.AddComponent<T>();
        _systemStack.Push(popup);
        go.transform.SetParent(SystemRoot.transform, false);
        go.transform.SetAsLastSibling();
        popup.Init();
        return popup;
    }

    public void CloseSystemUI()
    {
        if (_systemStack.Count > 0)
            Destroy(_systemStack.Pop().gameObject);
    }
}
