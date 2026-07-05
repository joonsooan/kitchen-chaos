using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// 테스트용 게임오버 — F9 (또는 TriggerGameOver 호출) 시:
// 현재 아이디+점수를 로컬(PlayerPrefs) 리더보드에 기록하고 리더보드 팝업 표시.
public class GameOverTester : MonoBehaviour
{
    private const string BoardKey = "Leaderboard";   // "id,score|id,score|..."
    private const int MaxRows = 3;

    private bool triggered;   // 페이즈 판정·재앙 판정 둘 다 쏴도 결과창 1번만

    private void OnEnable()
    {
        DisasterManager.OnDisasterGameOver += TriggerGameOver;
    }

    private void OnDisable()
    {
        DisasterManager.OnDisasterGameOver -= TriggerGameOver;
    }

    private int nextBuffIndex;   // F8 순환용

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f9Key.wasPressedThisFrame)
            TriggerGameOver(GameManager.Instance != null && GameManager.Instance.Score > 0);

        // F8 — 버프 강제 발동 (누를 때마다 다음 버프로 순환, 꽝 제외)
        if (Keyboard.current.f8Key.wasPressedThisFrame)
            ForceNextBuff();

        // F7 — 컵/접시 재고 소진 경고 강제 표시 (스테이션 순환)
        if (Keyboard.current.f7Key.wasPressedThisFrame)
            ForceStationWarning();

        // F5 — 장난 팝업 테스트 (연타하면 멘트 순환)
        if (Keyboard.current.f5Key.wasPressedThisFrame)
            ForceDisasterPopup();
    }

    private static readonly (string title, string desc)[] TestPranks =
    {
        ("양상추 대탈주", "양상추들이 도망가기 \n시작합니다!3분 동안 양상추를 획득 시 높은 확률로\n'양배추' 몬스터가 생성됩니다.\n공격하여 제거할 수 있습니다."),
        ("환각 버섯", "이상한 버섯을 먹었습니다!\n눈앞이 빙글빙글 돕니다...\n3분 동안 좌우 이동 키가 뒤바뀝니다."),
        ("잡초 번식", "잡초가 무성하게 자라납니다!\n3분 동안 잡초가 무작위 위치에 \n생성됩니다.공격하여 제거할 수 있습니다."),
    };
    private int nextPrankIndex;   // F5 순환용

    private void ForceDisasterPopup()
    {
        var (title, desc) = TestPranks[nextPrankIndex % TestPranks.Length];
        nextPrankIndex++;

        // 이미 장난 팝업이 떠 있으면 그것만 닫고 새로 (연타 시 멘트 교체 — 다른 팝업은 안 건드림)
        var existing = FindFirstObjectByType<DisasterPopup>();
        if (existing != null) UIManager.Instance.ClosePopupUI(existing);

        UIManager.Instance.ShowPopupUI<DisasterPopup>().Setup(title, desc, new Color(0.55f, 0.05f, 0.05f));
        Debug.Log($"[PrankTest] F5 팝업: {title}");
    }

    private int nextStationIndex;   // F7 순환용

    private void ForceStationWarning()
    {
        var bridge = GetComponent<OrderUIBridge>();
        var stations = FindObjectsByType<ReturnStation>(FindObjectsSortMode.None);
        if (bridge == null || stations.Length == 0) return;

        var station = stations[nextStationIndex % stations.Length];
        nextStationIndex++;

        bridge.HandleStationEmpty(station);
        Debug.Log($"[StationTest] F7 경고 표시: {station.name}");
    }

    private void ForceNextBuff()
    {
        if (BuffManager.Instance == null) return;

        var buffs = System.Array.FindAll(DataTable.Buffs, b => b.duration > 0f);
        if (buffs.Length == 0) return;

        var buff = buffs[nextBuffIndex % buffs.Length];
        nextBuffIndex++;

        BuffManager.Instance.Activate(buff);
        Debug.Log($"[BuffTest] F8 강제 발동: {buff.buffName} ({buff.duration}초)");
    }

    // 재앙 시스템의 최소 점수 판정(DisasterManager.OnDisasterGameOver) 또는 F9 테스트 키로 호출
    public void TriggerGameOver(bool success)
    {
        if (triggered) return;
        triggered = true;

        string id  = PlayerPrefs.GetString(NameInputPopup.PlayerIdKey, "Player");
        int score  = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        var records = LoadRecords();
        records.Add((id, score));
        SaveRecords(records);

        if (GameManager.Instance != null) GameManager.Instance.StopGame();

        var popup = UIManager.Instance.ShowPopupUI<LeaderboardPopup>();
        popup.Setup(success);
        popup.SetRows(FormatRows(records, id, score));
    }

    private List<(string id, int score)> LoadRecords()
    {
        var list = new List<(string, int)>();
        string raw = PlayerPrefs.GetString(BoardKey, string.Empty);
        if (string.IsNullOrEmpty(raw)) return list;

        foreach (string entry in raw.Split('|'))
        {
            var parts = entry.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[1], out int s))
                list.Add((parts[0], s));
        }
        return list;
    }

    private void SaveRecords(List<(string id, int score)> records)
    {
        // 상위 기록만 유지
        var top = records.OrderByDescending(r => r.score).Take(20);
        PlayerPrefs.SetString(BoardKey, string.Join("|", top.Select(r => $"{r.id},{r.score}")));
        PlayerPrefs.Save();
    }

    private string FormatRows(List<(string id, int score)> records, string myId, int myScore)
    {
        var top = records.OrderByDescending(r => r.score).Take(MaxRows).ToList();

        var lines = new List<string>();
        for (int i = 0; i < top.Count; i++)
        {
            string mark = (top[i].id == myId && top[i].score == myScore) ? " ◀" : "";
            lines.Add($"{i + 1:00} {top[i].id} {top[i].score}점{mark}");
        }
        return string.Join("\n", lines);
    }
}
