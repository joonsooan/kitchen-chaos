using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 튜토리얼 팝업 — 페이지 3장(전체 로직/레시피/조작법) 넘기고 마지막 '다음'에 닫힘
public class TutorialPopup : UIPopup
{
    enum Texts
    {
        DescText,
        NextText,
    }

    enum GameObjects
    {
        NextButton,
    }

    [SerializeField] private string[] pages =
    {
        "1. 전체 로직 설명",
        "2. 레시피 설명",
        "3. 조작법 설명",
    };

    private TextMeshProUGUI descText;
    private TextMeshProUGUI nextText;
    private int pageIndex;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        descText = Get<TextMeshProUGUI>((int)Texts.DescText);
        nextText = Get<TextMeshProUGUI>((int)Texts.NextText);

        BindEvent(Get<GameObject>((int)GameObjects.NextButton), OnNextClicked);

        pageIndex = 0;
        Refresh();
    }

    private void OnNextClicked(PointerEventData evt)
    {
        pageIndex++;

        if (pageIndex >= pages.Length)
        {
            UIManager.Instance.ClosePopupUI(this);
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (descText != null && pageIndex < pages.Length)
            descText.text = pages[pageIndex];

        // 마지막 장이면 버튼 라벨 교체
        if (nextText != null)
            nextText.text = pageIndex == pages.Length - 1 ? "시작" : "다음";
    }
}
