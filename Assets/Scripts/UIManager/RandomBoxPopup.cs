using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 랜덤박스 팝업 — 선물 클릭 → 흔들·펑 연출 → 버프 롤 → 결과 표시, X로 닫기
// 결과창 "다시 돌리기" — 코인 차감 후 선택창 복원, 잔액 부족 시 비활성
public class RandomBoxPopup : UIPopup
{
    enum Texts
    {
        DescText,
        RerollText,
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
        RerollButton,
    }

    private TextMeshProUGUI descText;
    private TextMeshProUGUI rerollText;
    private GameObject giftRow;
    private GameObject rerollButton;
    private UnityEngine.UI.Image rerollImage;
    private bool opened;
    private BuffData lastBuff;   // 이번 개봉에서 활성화한 버프 — 리롤 시 교체(해제) 대상

    private string initialDescText;
    private Color  initialDescColor;

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        descText     = Get<TextMeshProUGUI>((int)Texts.DescText);
        rerollText   = Get<TextMeshProUGUI>((int)Texts.RerollText);
        giftRow      = Get<GameObject>((int)GameObjects.GiftRow);
        rerollButton = Get<GameObject>((int)GameObjects.RerollButton);
        if (rerollButton != null) rerollImage = rerollButton.GetComponent<UnityEngine.UI.Image>();

        if (descText != null)
        {
            initialDescText  = descText.text;
            initialDescColor = descText.color;
        }

        BindEvent(Get<GameObject>((int)GameObjects.CloseButton), OnCloseClicked);
        if (rerollButton != null)
        {
            BindEvent(rerollButton, OnRerollClicked);
            rerollButton.SetActive(false);
        }

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
        lastBuff = buff;

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

        ShowRerollButton();
    }

    private void ShowRerollButton()
    {
        if (rerollButton == null || RandomBoxManager.Instance == null) return;

        int  cost       = RandomBoxManager.Instance.Cost;
        bool affordable = GameManager.Instance != null && GameManager.Instance.Money >= cost;

        if (rerollText != null)
        {
            rerollText.text  = $"다시 돌리기 ({cost}코인)";
            rerollText.color = affordable
                ? new Color(0.298f, 0.176f, 0.094f)
                : new Color(0.55f, 0.55f, 0.55f);
        }
        if (rerollImage != null)
            rerollImage.color = affordable
                ? new Color(0.95f, 0.75f, 0.2f)
                : new Color(0.6f, 0.6f, 0.6f);

        rerollButton.SetActive(true);

        rerollButton.transform.DOKill();
        rerollButton.transform.localScale = Vector3.zero;
        rerollButton.transform.DOScale(1f, 0.25f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .SetLink(rerollButton);
    }

    private void OnRerollClicked(PointerEventData evt)
    {
        if (!opened) return;
        if (RandomBoxManager.Instance == null) return;

        // 잔액 부족 — 버튼 흔들어서 거부 피드백
        if (!RandomBoxManager.Instance.TryPayReroll())
        {
            rerollButton.transform.DOKill(true);
            rerollButton.transform.DOShakePosition(0.3f, new Vector3(10f, 0f, 0f), 20)
                        .SetUpdate(true)
                        .SetLink(rerollButton);
            return;
        }

        // 리롤 = 교체 — 직전 개봉 버프 해제 후 새로 뽑기
        if (lastBuff != null && BuffManager.Instance != null)
            BuffManager.Instance.Deactivate(lastBuff);
        lastBuff = null;

        SoundManager.Instance?.PlaySFX(SFXType.RandomBoxOpen);
        ResetToSelection();
    }

    // 선택창 복원 — 선물 트랜스폼·안내 텍스트 원상 복구
    private void ResetToSelection()
    {
        opened = false;

        if (rerollButton != null) rerollButton.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            var gift = Get<GameObject>((int)GameObjects.Gift0 + i);
            if (gift == null) continue;

            var t = gift.transform;
            t.DOKill();
            t.localScale    = Vector3.one;
            t.localRotation = Quaternion.identity;
        }

        if (descText != null)
        {
            descText.transform.DOKill();
            descText.transform.localScale = Vector3.one;
            descText.text  = initialDescText;
            descText.color = initialDescColor;
        }

        if (giftRow != null) giftRow.SetActive(true);
    }

    private void OnCloseClicked(PointerEventData evt)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
