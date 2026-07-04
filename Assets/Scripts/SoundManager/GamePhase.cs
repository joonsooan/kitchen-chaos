/// <summary>
/// BGM 전환 기준이 되는 게임 페이즈.
/// SoundManager.SetPhase()로 변경 → BGM 자동 crossfade.
/// 순서(int)는 SoundLibrary.asset의 bgmEntries와 일치시킬 것.
/// </summary>
public enum GamePhase
{
    MainMenu,   // 메인 화면 (진입 트리거 미구현 — 예약)
    Stage,      // 스테이지 (게임 시작)
}
