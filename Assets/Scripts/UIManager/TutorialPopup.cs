using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 튜토리얼 팝업 — 노트북 프레임, 페이지 = Pages 아래 자식 GameObject (페이지별 자유 레이아웃).
// 마지막 페이지에서 다음 → 완료 이벤트 (브리지가 구독해 이름 입력으로 진행).
public class TutorialPopup : UIPopup
{
    // 튜토리얼 완료 — 다음 단계(이름 입력)가 구독
    public static event Action OnTutorialCompleted;

    enum GameObjects
    {
        PrevButton,
        NextButton,
        Pages,
    }

    public override bool PauseGameWhileOpen => true;

    private Transform pagesRoot;
    private GameObject prevButton;
    private int index;

    public override void Init()
    {
        CloseOnEsc = false;   // 필수 팝업 — ESC로 못 닫음

        Bind<GameObject>(typeof(GameObjects));

        pagesRoot  = Get<GameObject>((int)GameObjects.Pages).transform;
        prevButton = Get<GameObject>((int)GameObjects.PrevButton);

        BindEvent(prevButton, OnPrevClicked);
        BindEvent(Get<GameObject>((int)GameObjects.NextButton), OnNextClicked);

        Show(0);
    }

    private void Show(int i)
    {
        index = Mathf.Clamp(i, 0, pagesRoot.childCount - 1);

        for (int c = 0; c < pagesRoot.childCount; c++)
            pagesRoot.GetChild(c).gameObject.SetActive(c == index);

        // 첫 장에선 ◀ 숨김
        if (prevButton != null) prevButton.SetActive(index > 0);
    }

    private void OnPrevClicked(PointerEventData evt) => Show(index - 1);

    private void OnNextClicked(PointerEventData evt)
    {
        if (index >= pagesRoot.childCount - 1) Complete();
        else Show(index + 1);
    }

    // 마지막 페이지 '시작' — 닫고 완료 신호 (이름 입력 팝업으로 이어짐)
    private void Complete()
    {
        UIManager.Instance.ClosePopupUI(this);
        OnTutorialCompleted?.Invoke();
    }
}
