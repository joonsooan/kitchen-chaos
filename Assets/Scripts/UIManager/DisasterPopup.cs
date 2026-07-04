using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 장난 발생 팝업 — 제목/설명 표시, X로 닫기. 장난 시스템이 Setup 호출.
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

    public override bool PauseGameWhileOpen => true;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        titleText = Get<TextMeshProUGUI>((int)Texts.TitleText);
        descText  = Get<TextMeshProUGUI>((int)Texts.DescText);

        BindEvent(Get<GameObject>((int)GameObjects.CloseButton), OnCloseClicked);

        // 장난 팝업이 실제로 표시(ShowPopupUI<DisasterPopup>)되면 자동 발화. 표시 트리거 미구현이라 지금은 휴면.
        SoundManager.Instance?.PlaySFX(SFXType.DisasterOpen);

        PlayDisasterFx();
    }

    // 장난 임팩트 — 카메라 흔들림 + 딤 붉은 플래시
    private void PlayDisasterFx()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.DOKill(true);
            cam.transform.DOShakePosition(0.45f, 0.35f, 18)
               .SetUpdate(true)
               .SetLink(cam.gameObject);
        }

        var dim = GetComponent<UnityEngine.UI.Image>();
        if (dim != null)
        {
            float baseAlpha = dim.color.a;
            var seq = DG.Tweening.DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            seq.Append(dim.DOColor(new Color(0.55f, 0.05f, 0.05f, 0.5f), 0.12f));
            seq.Append(dim.DOColor(new Color(0f, 0f, 0f, baseAlpha), 0.4f));
        }
    }

    // 장난 정보 주입 — 예: Setup("어패류 장난 발생", "1분간 재료 조달이 막힙니다.")
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
