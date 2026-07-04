using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 튜토리얼 팝업 — 좌 텍스트/우 이미지, ◀▶로 페이지 이동.
// 마지막 페이지에서 아이디 입력 → 저장 후 닫히며 OnTutorialCompleted 발행 (게임 시작 신호).
public class TutorialPopup : UIPopup
{
    // 튜토리얼 완료 — 다음 단계(이름 입력)가 구독
    public static event Action OnTutorialCompleted;

    [Serializable]
    public class Page
    {
        public string title;
        [TextArea] public string desc;
        public Sprite image;
    }

    enum Texts
    {
        TitleText,
        DescText,
    }

    enum Images
    {
        TutorialImage,
    }

    enum GameObjects
    {
        PrevButton,
        NextButton,
    }

    [SerializeField] private Page[] pages;

    public override bool PauseGameWhileOpen => true;

    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;
    private Image tutorialImage;
    private GameObject prevButton;
    private int pageIndex;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));

        titleText     = Get<TextMeshProUGUI>((int)Texts.TitleText);
        descText      = Get<TextMeshProUGUI>((int)Texts.DescText);
        tutorialImage = Get<Image>((int)Images.TutorialImage);
        prevButton    = Get<GameObject>((int)GameObjects.PrevButton);

        BindEvent(prevButton, OnPrevClicked);
        BindEvent(Get<GameObject>((int)GameObjects.NextButton), OnNextClicked);

        pageIndex = 0;
        Refresh();
    }

    private bool IsLastPage => pages != null && pageIndex >= pages.Length - 1;

    private void OnPrevClicked(PointerEventData evt)
    {
        if (pageIndex == 0) return;
        pageIndex--;
        Refresh();
    }

    private void OnNextClicked(PointerEventData evt)
    {
        if (IsLastPage)
        {
            Complete();
            return;
        }
        pageIndex++;
        Refresh();
    }

    // 마지막 페이지 '시작' — 닫고 완료 신호 (이름 입력 팝업으로 이어짐)
    private void Complete()
    {
        UIManager.Instance.ClosePopupUI(this);
        OnTutorialCompleted?.Invoke();
    }

    private void Refresh()
    {
        if (pages == null || pages.Length == 0) return;
        var page = pages[Mathf.Clamp(pageIndex, 0, pages.Length - 1)];

        if (titleText != null) titleText.text = page.title;
        if (descText != null)  descText.text  = page.desc;

        if (tutorialImage != null && page.image != null)
            tutorialImage.sprite = page.image;

        // 첫 장에선 ◀ 숨김
        if (prevButton != null) prevButton.SetActive(pageIndex > 0);
    }
}
