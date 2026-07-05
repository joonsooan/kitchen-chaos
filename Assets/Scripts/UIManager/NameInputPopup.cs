using System;
using TMPro;
using UnityEngine;

// 이름 입력 팝업 — 튜토리얼 완료 후 표시. 엔터로 확정 → 저장 → 게임 시작 신호.
public class NameInputPopup : UIPopup
{
    public const string PlayerIdKey = "PlayerId";

    // 이름 확정 — 게임 시작 트리거가 구독
    public static event Action OnNameConfirmed;

    enum GameObjects
    {
        NameInput,
    }

    public override bool PauseGameWhileOpen => true;

    private TMP_InputField nameField;

    public override void Init()
    {
        CloseOnEsc = false;   // 필수 팝업 — ESC로 못 닫음

        // 이름 타이핑 중 WASD(방향 전환)/F(상호작용) 차단
        FindFirstObjectByType<PlayerMovement>()?.SetInputEnabled(false);
        Bind<GameObject>(typeof(GameObjects));

        var input = Get<GameObject>((int)GameObjects.NameInput);
        if (input != null) nameField = input.GetComponent<TMP_InputField>();

        if (nameField != null)
        {
            nameField.onSubmit.AddListener(_ => Confirm());
            nameField.Select();
            nameField.ActivateInputField();
        }
    }

    private void Confirm()
    {
        string id = nameField != null ? nameField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(id)) id = "Player";

        PlayerPrefs.SetString(PlayerIdKey, id);
        PlayerPrefs.Save();

        UIManager.Instance.ClosePopupUI(this);
        OnNameConfirmed?.Invoke();
    }
    private void OnDestroy()
    {
        // 팝업 닫히면 입력 복원
        FindFirstObjectByType<PlayerMovement>()?.SetInputEnabled(true);
    }

}
