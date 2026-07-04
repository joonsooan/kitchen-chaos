using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 재앙 발생 팝업 — 제목/설명 표시, X로 닫기. 재앙 시스템이 Setup 호출.
public class DisasterPopup : UIPopup
{
    enum Texts
    {
        TitleText,
        DescText,
    }

    enum GameObjects
    {
        CloseButton,
    }

    private TextMeshProUGUI titleText;
    private TextMeshProUGUI descText;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        titleText = Get<TextMeshProUGUI>((int)Texts.TitleText);
        descText  = Get<TextMeshProUGUI>((int)Texts.DescText);

        BindEvent(Get<GameObject>((int)GameObjects.CloseButton), OnCloseClicked);

        // 재앙 팝업이 실제로 표시(ShowPopupUI<DisasterPopup>)되면 자동 발화. 표시 트리거 미구현이라 지금은 휴면.
        SoundManager.Instance?.PlaySFX(SFXType.DisasterOpen);
    }

    // 재앙 정보 주입 — 예: Setup("어패류 재앙 발생", "1분간 재료 조달이 막힙니다.")
    public void Setup(string title, string description)
    {
        if (titleText != null) titleText.text = title;
        if (descText != null)  descText.text  = description;
    }

    private void OnCloseClicked(PointerEventData evt)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
