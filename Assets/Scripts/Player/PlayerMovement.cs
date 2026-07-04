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
    private InputAction attackAction;

    private Vector2 moveInput;

    public Vector2 FacingDirection { get; private set; } = Vector2.down;
    public event Action InteractPressed;
    public event Action AttackPressed;

    // 재난 이벤트(키보드 반전)가 켜고 끄는 플래그. 상하좌우 입력을 벡터 negate로 동시 반전.
    public bool InputInverted { get; set; }

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

        // 상호작용 키 = F
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/f");

        // 타격(공격) 키 = Q. 양배추 괴물·잡초 타격 전용.
        attackAction = new InputAction("Attack", InputActionType.Button, "<Keyboard>/q");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        interactAction.Enable();
        attackAction.Enable();
        interactAction.performed += OnInteractPerformed;
        attackAction.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        attackAction.performed -= OnAttackPerformed;
        moveAction.Disable();
        interactAction.Disable();
        attackAction.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        if (InputInverted) moveInput = -moveInput;
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

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPressed?.Invoke();
    }
}
