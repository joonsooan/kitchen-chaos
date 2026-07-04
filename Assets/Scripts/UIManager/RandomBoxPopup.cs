using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 랜덤박스 팝업 — 선물 클릭 → 버프 롤 → 결과 표시, X로 닫기
public class RandomBoxPopup : UIPopup
{
    enum Texts
    {
        DescText,
    }

    enum GameObjects
    {
        CloseButton,
        GiftRow,
        Gift0,
        Gift1,
        Gift2,
        Gift3,
    }

    private TextMeshProUGUI descText;
    private GameObject giftRow;
    private bool opened;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        descText = Get<TextMeshProUGUI>((int)Texts.DescText);
        giftRow  = Get<GameObject>((int)GameObjects.GiftRow);

        BindEvent(Get<GameObject>((int)GameObjects.CloseButton), OnCloseClicked);
        BindEvent(Get<GameObject>((int)GameObjects.Gift0), evt => OnGiftClicked());
        BindEvent(Get<GameObject>((int)GameObjects.Gift1), evt => OnGiftClicked());
        BindEvent(Get<GameObject>((int)GameObjects.Gift2), evt => OnGiftClicked());
        BindEvent(Get<GameObject>((int)GameObjects.Gift3), evt => OnGiftClicked());
    }

    private void OnGiftClicked()
    {
        if (opened) return;   // 1회만 개봉
        opened = true;

        BuffData buff = RandomBoxManager.Instance != null
            ? RandomBoxManager.Instance.Roll()
            : null;

        if (buff != null && BuffManager.Instance != null)
            BuffManager.Instance.Activate(buff);

        // 결과 표시 — 선물 숨기고 결과 텍스트로 전환
        if (giftRow != null) giftRow.SetActive(false);
        if (descText != null)
        {
            descText.text = buff != null
                ? $"{buff.buffName}\n\n{buff.description}"
                : "꽝! 아무것도 들어있지 않았다...";
        }
    }

    private void OnCloseClicked(PointerEventData evt)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
