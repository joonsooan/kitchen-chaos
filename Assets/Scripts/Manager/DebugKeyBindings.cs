#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DebugKeyBindings : MonoBehaviour
{
    private InputAction restartAction;
    private InputAction speed1Action;
    private InputAction speed2Action;
    private InputAction speed3Action;
    private InputAction speed4Action;

    private void Awake()
    {
        restartAction = new InputAction("DebugRestart", InputActionType.Button, "<Keyboard>/r");
        speed1Action = new InputAction("DebugSpeed1", InputActionType.Button, "<Keyboard>/1");
        speed2Action = new InputAction("DebugSpeed2", InputActionType.Button, "<Keyboard>/2");
        speed3Action = new InputAction("DebugSpeed3", InputActionType.Button, "<Keyboard>/3");
        speed4Action = new InputAction("DebugSpeed4", InputActionType.Button, "<Keyboard>/4");
    }

    private void OnEnable()
    {
        restartAction.Enable();
        speed1Action.Enable();
        speed2Action.Enable();
        speed3Action.Enable();
        speed4Action.Enable();

        restartAction.performed += OnRestartPerformed;
        speed1Action.performed += OnSpeed1Performed;
        speed2Action.performed += OnSpeed2Performed;
        speed3Action.performed += OnSpeed3Performed;
        speed4Action.performed += OnSpeed4Performed;
    }

    private void OnDisable()
    {
        restartAction.performed -= OnRestartPerformed;
        speed1Action.performed -= OnSpeed1Performed;
        speed2Action.performed -= OnSpeed2Performed;
        speed3Action.performed -= OnSpeed3Performed;
        speed4Action.performed -= OnSpeed4Performed;

        restartAction.Disable();
        speed1Action.Disable();
        speed2Action.Disable();
        speed3Action.Disable();
        speed4Action.Disable();
    }

    private void OnRestartPerformed(InputAction.CallbackContext context)
    {
        if (TypingGuard.IsTyping) return;   // 이름 입력 중 'r' 타이핑 보호

        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    private void OnSpeed1Performed(InputAction.CallbackContext context) => SetTimeScale(1f);
    private void OnSpeed2Performed(InputAction.CallbackContext context) => SetTimeScale(2f);
    private void OnSpeed3Performed(InputAction.CallbackContext context) => SetTimeScale(3f);
    private void OnSpeed4Performed(InputAction.CallbackContext context) => SetTimeScale(4f);

    private void SetTimeScale(float scale)
    {
        if (TypingGuard.IsTyping) return;   // 이름 입력 중 숫자 타이핑 보호
        Time.timeScale = scale;
    }
}
#endif
