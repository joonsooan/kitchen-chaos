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

    public override void Init()
    {
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
        if (titleText != null) titleText.text = success ? "Success!" : "Fail";
        if (subText != null)   subText.text   = success ? "당신은 최고의 셰프입니다" : "까비까비";
    }

    // 순위 데이터 연동용 — 한 줄당 한 순위
    public void SetRows(string rows)
    {
        if (rowsText != null) rowsText.text = rows;
    }

    private void OnMainClicked(PointerEventData evt)
    {
        // TODO: 메인 화면 씬/상태 흐름 확정되면 전환 연결
        Debug.Log("[LeaderboardPopup] 메인 화면으로 (흐름 미정)");
        UIManager.Instance.ClosePopupUI(this);
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
