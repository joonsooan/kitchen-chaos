using UnityEngine;

// 잠잠한 하루 — 효과 없음(팝업만). 재앙 풀의 '휴식' 슬롯.
public class CalmDayEvent : DisasterEvent
{
    public override float Duration => 60f;               // 효과는 없지만 한 페이즈 길이만큼 유지
    public override Color PopupFlashColor => SkyFlash;   // 하늘색

    protected override bool TryTrigger() => true;
}
