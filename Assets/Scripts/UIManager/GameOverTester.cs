using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

// 테스트용 게임오버 — F9 (또는 TriggerGameOver 호출) 시:
// 현재 아이디+점수를 로컬(PlayerPrefs) 리더보드에 기록하고 리더보드 팝업 표시.
public class GameOverTester : MonoBehaviour
{
    private const string BoardKey = "Leaderboard";   // "id,score|id,score|..."
    private const int MaxRows = 5;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            TriggerGameOver();
    }

    // 실제 게임오버 로직이 생기면 그쪽에서 이 메서드 호출
    public void TriggerGameOver()
    {
        string id  = PlayerPrefs.GetString(NameInputPopup.PlayerIdKey, "Player");
        int score  = GameManager.Instance != null ? GameManager.Instance.Score : 0;

        var records = LoadRecords();
        records.Add((id, score));
        SaveRecords(records);

        if (GameManager.Instance != null) GameManager.Instance.StopGame();

        bool success = score > 0;   // 성공 기준 미정 — 임시로 점수>0
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
            lines.Add($"{i + 1:00}   {top[i].id}   {top[i].score}{mark}");
        }
        return string.Join("\n", lines);
    }
}
