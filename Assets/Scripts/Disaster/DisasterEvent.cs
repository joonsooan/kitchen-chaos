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

    public bool Trigger()
    {
        if (!TryTrigger()) return false;

        OnAnyDisasterTriggered?.Invoke(this);
        return true;
    }

    protected abstract bool TryTrigger();
}
