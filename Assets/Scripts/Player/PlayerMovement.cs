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

    private InputSystem_Actions inputActions;
    private CharacterController controller;
    private Vector2 moveInput;

    public float RotationSpeed => rotationSpeed;
    public float AngleToStartMoving => angleToStartMoving;

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
        HandleMovementAndRotation();
    }

    private void ReadInput() {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void HandleMovementAndRotation()
    {
        if (moveInput.sqrMagnitude < 0.01f) return;

        Vector3 targetDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        float currentAngleDifference = Vector3.Angle(transform.forward, targetDirection);

        if (currentAngleDifference <= angleToStartMoving)
        {
            controller.Move(transform.forward * moveSpeed * Time.deltaTime);
        }
    }
}