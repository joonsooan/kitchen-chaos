using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 랜덤박스 팝업 — 선물 클릭 → 흔들·펑 연출 → 버프 롤 → 결과 표시, X로 닫기
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
        Gift4,
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

        for (int i = 0; i < 5; i++)
        {
            var gift = Get<GameObject>((int)GameObjects.Gift0 + i);
            BindEvent(gift, evt => OnGiftClicked(gift));
        }
    }

    private void OnGiftClicked(GameObject gift)
    {
        if (opened) return;   // 1회만 개봉
        opened = true;

        // 흔들흔들 → 커졌다 → 펑(축소 소멸) → 결과 공개
        var t   = gift.transform;
        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        seq.Append(t.DOShakeRotation(0.6f, new Vector3(0f, 0f, 30f), 20, 90f));
        seq.Join(t.DOShakePosition(0.6f, new Vector3(8f, 8f, 0f), 20));
        seq.Append(t.DOScale(1.45f, 0.14f).SetEase(Ease.OutQuad));
        seq.Append(t.DOScale(0f, 0.12f).SetEase(Ease.InBack));
        seq.OnComplete(Reveal);
    }

    private void Reveal()
    {
        BuffData buff = RandomBoxManager.Instance != null
            ? RandomBoxManager.Instance.Roll()
            : null;

        if (buff != null && BuffManager.Instance != null)
            BuffManager.Instance.Activate(buff);

        // 결과 표시 — 선물 숨기고 결과 텍스트 뿅 + 등급색 (꽝 회색 / 버프 금색)
        if (giftRow != null) giftRow.SetActive(false);
        if (descText != null)
        {
            bool isEmpty = buff == null || buff.duration <= 0f;

            descText.text = buff != null
                ? $"{buff.buffName}\n\n{buff.description}"
                : "꽝! 아무것도 들어있지 않았다...";
            descText.color = isEmpty
                ? new Color(0.45f, 0.45f, 0.45f)
                : new Color(0.85f, 0.65f, 0.1f);

            descText.transform.DOKill();
            descText.transform.localScale = Vector3.zero;
            descText.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .SetLink(descText.gameObject);
        }
    }

    private void OnCloseClicked(PointerEventData evt)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
