using System;
using UnityEngine;

// 임시 페이즈 시스템 — 3분 경과마다 현재 점수 >= 목표 점수 확인.
//   미달: 즉시 게임오버(실패, 결과창). 충족: 다음 페이즈로 조용히 진행.
//   4페이즈 종료 시점엔 성공/실패 판정으로 결과창.
// 팀에서 정식 페이즈 이벤트 만들면 이 파일 삭제하고 그쪽 구독으로 교체.
[RequireComponent(typeof(GameOverTester))]
public class PhaseManager : MonoBehaviour
{
    // (페이즈 번호 1~4, 목표 점수) — 전환 시 발행 (UI 표시는 현재 안 씀)
    public static event Action<int, int> OnPhaseChanged;

    public static int CurrentPhase  { get; private set; }
    public static int CurrentTarget { get; private set; }

    private static readonly int[] Targets = { 50, 100, 150, 200 };
    [SerializeField] private float phaseSeconds = 180f;   // 페이즈 길이 (테스트 시 인스펙터에서 줄이기)

    private GameOverTester gameOver;
    private bool ended;

    private void Awake()
    {
        gameOver = GetComponent<GameOverTester>();
    }

    private void OnEnable()
    {
        CurrentPhase  = 0;   // 씬 리로드 시 리셋
        CurrentTarget = 0;
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

        int phase = Mathf.Min((int)(elapsedTime / phaseSeconds) + 1, Targets.Length + 1);
        if (phase == CurrentPhase) return;

        int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        // 방금 끝난 페이즈의 목표 미달 → 실패 결과창
        if (CurrentPhase >= 1 && score < Targets[CurrentPhase - 1])
        {
            EndGame(false);
            return;
        }

        // 4페이즈까지 마쳤으면 최종 판정 (여기 왔으면 목표 충족 = 성공)
        if (phase > Targets.Length)
        {
            EndGame(true);
            return;
        }

        CurrentPhase  = phase;
        CurrentTarget = Targets[phase - 1];
        OnPhaseChanged?.Invoke(phase, CurrentTarget);
    }

    private void EndGame(bool success)
    {
        ended = true;
        if (gameOver != null) gameOver.TriggerGameOver(success);
    }
}
