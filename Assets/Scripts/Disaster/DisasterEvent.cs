using System;
using UnityEngine;

public abstract class DisasterEvent : MonoBehaviour
{
    // 재앙 발동 알림 — UI(재앙 팝업)가 구독
    public static event Action<DisasterEvent> OnDisasterTriggered;

    [SerializeField] private string title = "재앙 발생";
    [TextArea] [SerializeField] private string description = "( 재앙 설명 )";

    public string Title       => title;
    public string Description => description;

    public void Trigger()
    {
        Execute();
        OnDisasterTriggered?.Invoke(this);
    }

    protected abstract void Execute();
}
