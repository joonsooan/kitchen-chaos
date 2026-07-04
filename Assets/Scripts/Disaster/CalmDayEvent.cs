using UnityEngine;

// 잠잠한 하루 — 효과 없음(팝업만). 재앙 풀의 '휴식' 슬롯.
public class CalmDayEvent : DisasterEvent
{
    public override Color PopupFlashColor => SkyFlash;   // 하늘색

    protected override bool TryTrigger() => true;
}
