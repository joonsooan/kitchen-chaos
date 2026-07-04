using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 엔딩 + 리더보드 팝업 — Setup(success)으로 Success!/Fail 전환.
// 순위 데이터 이벤트가 생기면 SetRows로 채움 (지금은 프리팹 placeholder).
public class LeaderboardPopup : UIPopup
{
    enum Texts
    {
        TitleText,
        SubText,
        RowsText,
    }

    enum GameObjects
    {
        MainButton,
        QuitButton,
    }

    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subText;
    private TextMeshProUGUI rowsText;

    // 결과창 떠 있는 동안 게임 정지 (UIManager가 open/close 시 timeScale 관리)
    public override bool PauseGameWhileOpen => true;

    public override void Init()
    {
        CloseOnEsc = false;   // 게임오버 화면 — 메인/종료 중 선택해야 함

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        titleText = Get<TextMeshProUGUI>((int)Texts.TitleText);
        subText   = Get<TextMeshProUGUI>((int)Texts.SubText);
        rowsText  = Get<TextMeshProUGUI>((int)Texts.RowsText);

        BindEvent(Get<GameObject>((int)GameObjects.MainButton), OnMainClicked);
        BindEvent(Get<GameObject>((int)GameObjects.QuitButton), OnQuitClicked);
    }

    // 게임 종료 시 호출 — 성공/실패에 따라 문구 전환
    public void Setup(bool success)
    {
        if (titleText != null) titleText.text = success ? "Success" : "Fail";
        if (subText != null)   subText.text   = success ? "숲속 최고의 식당으로 인정받았습니다!" : "오늘은 운이 따라주지 않았습니다...";
    }

    // 순위 데이터 연동용 — 한 줄당 한 순위
    public void SetRows(string rows)
    {
        if (rowsText != null) rowsText.text = rows;
    }

    private void OnMainClicked(PointerEventData evt)
    {
        GoToTitle();
    }

    // 씬 리로드 → 게임 상태 초기화 + 브리지가 타이틀(HomeHUD) 다시 표시
    public static void GoToTitle()
    {
        UIManager.Instance.CloseAllPopupUI();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void OnQuitClicked(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
