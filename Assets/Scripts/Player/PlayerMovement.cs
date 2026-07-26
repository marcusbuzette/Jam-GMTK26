using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 720f; // Graus por segundo
    [Tooltip("O player só começa a andar se a diferença entre a direção que ele olha e o input for menor que este ângulo.")]
    [SerializeField] private float angleToStartMoving = 20f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    private InputSystem_Actions inputActions;
    private CharacterController controller;
    private Vector2 moveInput;
    private float verticalVelocity;

    public float RotationSpeed => rotationSpeed;
    public float AngleToStartMoving => angleToStartMoving;
    public float MoveSpeed => moveSpeed;
    public bool IsWalking => controller != null && controller.enabled && controller.velocity.sqrMagnitude > 0.01f;

    private void Awake() {
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable() {
        inputActions.Player.Enable();
    }

    private void OnDisable() {
        inputActions.Player.Disable();
    }

    private void Update() {
        ReadInput();
        ApplyGravity();
        HandleMovementAndRotation();
    }

    private void ReadInput() {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void HandleMovementAndRotation() {
        Vector3 horizontalMotion = Vector3.zero;

        if (moveInput.sqrMagnitude >= 0.01f) {
            Vector3 targetDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float currentAngleDifference = Vector3.Angle(transform.forward, targetDirection);
            if (currentAngleDifference <= angleToStartMoving) {
                horizontalMotion = transform.forward * moveSpeed;
            }
        }

        Vector3 motion = horizontalMotion + Vector3.up * verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }

    private void ApplyGravity() {
        if (controller.isGrounded) {
            if (verticalVelocity < 0f) {
                verticalVelocity = groundedGravity;
            }
        } else {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public void EnableMovement(bool enable) {
        enabled = enable;
        if (enable) {
            inputActions.Player.Enable(); // Habilita o input de movimento
        } else {
            inputActions.Player.Disable(); // Desabilita o input de movimento
        }
    }
}