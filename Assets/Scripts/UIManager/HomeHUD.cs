using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 타이틀(홈) 화면 — 시작/설정/종료. OrderUIBridge가 Play 직후 표시.
public class HomeHUD : UIHUD
{
    // 시작 버튼 → 브리지가 구독해 튜토리얼로 진행
    public static event Action OnStartRequested;

    enum GameObjects
    {
        StartButton,
        SettingButton,
        CloseButton,
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));

        BindEvent(Get<GameObject>((int)GameObjects.StartButton),   OnStartClicked);
        BindEvent(Get<GameObject>((int)GameObjects.SettingButton), OnSettingClicked);
        BindEvent(Get<GameObject>((int)GameObjects.CloseButton),   OnQuitClicked);
    }

    private void OnStartClicked(PointerEventData evt)
    {
        UIManager.Instance.HideHUDUI<HomeHUD>();
        OnStartRequested?.Invoke();
    }

    private void OnSettingClicked(PointerEventData evt)
    {
        UIManager.Instance.ShowPopupUI<OptionPopup>();
    }

    private void OnQuitClicked(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
