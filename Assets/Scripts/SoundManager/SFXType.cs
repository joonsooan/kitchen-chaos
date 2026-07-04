/// <summary>
/// SFX 종류 enum. SoundManager.PlaySFX(SFXType)로 호출.
/// 호출부는 AudioClip을 직접 들고 다닐 필요 없음.
/// 순서(int)는 SoundLibrary.asset의 sfxEntries와 일치시킬 것.
/// </summary>
public enum SFXType
{
    UIClick,        // 기본 UI 클릭
    PopupOpen,      // 팝업 열림 (공용)
    RandomBoxOpen,  // 랜덤박스 개봉 팝업
    DisasterOpen,   // 재앙 발생 팝업 (표시 트리거 미구현 — 휴면)
    OrderCreated,   // 주문 발생 (손님 착석)
    OrderFailed,    // 주문 실패 (인내심 소진)
    IngredientPick, // 재료 픽업 (바구니 등)
    Chop,           // 도마
    Fry,            // 후라이펜
    Mix,            // 믹서기
    Trash,          // 쓰레기통
    RewardGain,     // 점수·코인 획득 (서빙 성공)
    Serve,          // 퇴식구 제출
    Hit,            // 타격 (양배추 몬스터)
    LettuceVoice,   // 양상추 말소리 (시스템 미구현 — 예약)
}
