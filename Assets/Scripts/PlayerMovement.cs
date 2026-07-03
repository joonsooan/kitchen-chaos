using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D rb;
    private PlayerController playerController;

    private InputAction moveAction;
    private InputAction interactAction;

    private Vector2 moveInput;

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

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        playerController.ChangeState(moveInput.sqrMagnitude > 0f ? PlayerState.Moving : PlayerState.Idle);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * (moveSpeed * Time.fixedDeltaTime));
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        playerController.ChangeState(PlayerState.Fetching);
    }
}
