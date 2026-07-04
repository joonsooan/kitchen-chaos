using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 튜토리얼 팝업 — 좌 텍스트/우 이미지, ◀▶로 페이지 이동. 마지막 ▶에 닫힘.
public class TutorialPopup : UIPopup
{
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

    private void OnPrevClicked(PointerEventData evt)
    {
        if (pageIndex == 0) return;
        pageIndex--;
        Refresh();
    }

    private void OnNextClicked(PointerEventData evt)
    {
        if (pageIndex >= pages.Length - 1)
        {
            UIManager.Instance.ClosePopupUI(this);
            return;
        }
        pageIndex++;
        Refresh();
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
