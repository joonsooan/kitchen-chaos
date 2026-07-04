using System;
using UnityEngine;

public abstract class DisasterEvent : MonoBehaviour
{
    [SerializeField] private string disasterName;
    [SerializeField, TextArea] private string disasterDescription;

    public static event Action<DisasterEvent> OnAnyDisasterTriggered;

    public string DisasterName => disasterName;
    public string DisasterDescription => disasterDescription;
    public virtual float Duration => 0f;
    public virtual bool ShowsPopup => true;

    // 팝업 딤 플래시 색 — 재앙=빨강, 긍정/중립 이벤트는 SkyFlash로 override.
    public static readonly Color RedFlash = new Color(0.55f, 0.05f, 0.05f, 0.5f);
    public static readonly Color SkyFlash = new Color(0.35f, 0.72f, 1f, 0.5f);
    public virtual Color PopupFlashColor => RedFlash;

    public bool Trigger()
    {
        if (!TryTrigger()) return false;

        OnAnyDisasterTriggered?.Invoke(this);
        return true;
    }

    // 빌딩(재료 바구니) 상호작용 훅 — 기본 no-op. 양배추 이벤트가 뽑기 스폰에 사용.
    public virtual void OnBuildingInteract(PlayerController player) { }

    protected abstract bool TryTrigger();
}
