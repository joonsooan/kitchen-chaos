using System;
using UnityEngine;

// 임시 페이즈 시스템 — 3분 페이즈 → 2분 쉬는 시간 반복, 4페이즈까지.
// 시간 구간·HUD 표시 전용. 점수 판정·게임오버는 DisasterManager가 재앙마다 수행.
// 팀에서 정식 페이즈 이벤트 만들면 이 파일 삭제하고 그쪽 구독으로 교체.
public class PhaseManager : MonoBehaviour
{
    // (페이즈 번호 1~4, 목표 점수) — 페이즈 시작 시 발행
    public static event Action<int, int> OnPhaseChanged;

    public static int   CurrentPhase     { get; private set; }
    public static int   CurrentTarget    { get; private set; }
    public static bool  IsResting        { get; private set; }
    public static float SegmentRemaining { get; private set; }   // 현재 구간(페이즈/휴식) 남은 초

    private static readonly int[] Targets = { 50, 100, 150, 200 };

    [SerializeField] private float phaseSeconds = 180f;   // 페이즈 길이 (테스트 시 인스펙터에서 줄이기)
    [SerializeField] private float restSeconds  = 120f;   // 쉬는 시간 길이

    private int  lastSegment = -1;   // 0=1페이즈, 1=휴식, 2=2페이즈, 3=휴식 ... 6=4페이즈
    private bool ended;

    private void OnEnable()
    {
        CurrentPhase = 0;   // 씬 리로드 시 리셋
        CurrentTarget = 0;
        IsResting = false;
        SegmentRemaining = 0f;
        lastSegment = -1;
        ended = false;
        GameManager.OnTimeTick += HandleTimeTick;
    }

    private void OnDisable()
    {
        GameManager.OnTimeTick -= HandleTimeTick;
    }

    private void HandleTimeTick(float elapsedTime)
    {
        if (ended) return;

        float cycle   = phaseSeconds + restSeconds;
        int   k       = (int)(elapsedTime / cycle);          // 몇 번째 사이클
        float inCycle = elapsedTime - k * cycle;
        bool  inPhase = inCycle < phaseSeconds;
        int   segment = k * 2 + (inPhase ? 0 : 1);

        SegmentRemaining = inPhase ? phaseSeconds - inCycle : cycle - inCycle;

        if (segment == lastSegment) return;
        lastSegment = segment;

        // 4페이즈(segment 6) 종료 → 페이즈 진행만 정지 (게임오버는 DisasterManager 담당)
        if (segment >= 7)
        {
            ended = true;
            return;
        }

        if (segment % 2 == 0)
        {
            // 페이즈 시작
            IsResting     = false;
            CurrentPhase  = segment / 2 + 1;
            CurrentTarget = Targets[CurrentPhase - 1];
            OnPhaseChanged?.Invoke(CurrentPhase, CurrentTarget);
        }
        else
        {
            // 페이즈 종료 → 휴식 진입 (판정 없음)
            int endedPhase = (segment + 1) / 2;
            IsResting = true;
            // 휴식 동안엔 다음 페이즈 목표를 미리 표시 (표시 전용)
            if (endedPhase < Targets.Length) CurrentTarget = Targets[endedPhase];
        }
    }
}
