using UnityEngine.EventSystems;

// 텍스트 입력 필드(이름 입력 등)에 포커스가 있는 동안 게임 입력을 막기 위한 공용 판별.
// 사용처: PlayerMovement(방향 전환), DebugKeyBindings(R 재시작/속도 키)
public static class TypingGuard
{
    public static bool IsTyping
    {
        get
        {
            var es = EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null) return false;
            return es.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;
        }
    }
}
