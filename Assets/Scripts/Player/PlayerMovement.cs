using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    // 버프용 이동속도 배율 (SpeedBuff가 설정)
    public float SpeedMultiplier { get; set; } = 1f;

    private Rigidbody2D rb;
    private PlayerController playerController;

    private InputAction moveAction;
    private InputAction interactAction;

    private Vector2 moveInput;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public event Action InteractPressed;

    // 재난 이벤트(키보드 반전)가 켜고 끄는 플래그. 좌우 입력만 반전(상하는 그대로).
    public bool InputInverted { get; set; }

    // 도마 등 requiresPresence 조리 중엔 false — 이동/상호작용 입력 자체를 막음.
    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // 상호작용/공격 키 = F (양배추 괴물·잡초 타격도 동일 키로 통합)
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        moveAction.Disable();
        interactAction.Disable();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (enabled) interactAction.Enable();
        else interactAction.Disable();
    }

    private void Update()
    {
        moveInput = inputEnabled ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        if (inputEnabled && InputInverted) moveInput.x = -moveInput.x;
        if (moveInput.sqrMagnitude > 0f) FacingDirection = moveInput.normalized;

        if (playerController.CurrentState == PlayerState.Busy) return;
        playerController.ChangeState(moveInput.sqrMagnitude > 0f ? PlayerState.Moving : PlayerState.Idle);
    }

    private void FixedUpdate()
    {
        if (playerController.CurrentState == PlayerState.Busy) return;
        rb.MovePosition(rb.position + moveInput.normalized * (moveSpeed * SpeedMultiplier * Time.fixedDeltaTime));
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        InteractPressed?.Invoke();
    }
}
