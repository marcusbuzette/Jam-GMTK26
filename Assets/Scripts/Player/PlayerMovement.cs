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

        // Vetor de direção baseado no mundo (W/S move no eixo Z, A/D move no eixo X)
        // Se a sua câmera for isométrica/rotacionada, você precisaria multiplicar essa direção pela rotação da câmera.
        Vector3 targetDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // 1. Lida com a rotação
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 2. Calcula a diferença entre onde o player está olhando e para onde ele quer ir
        float currentAngleDifference = Vector3.Angle(transform.forward, targetDirection);

        // 3. Só aplica o movimento se o personagem já estiver virado o suficiente
        if (currentAngleDifference <= angleToStartMoving)
        {
            // Usamos transform.forward para garantir que ele ande na direção em que o modelo está apontando
            controller.Move(transform.forward * moveSpeed * Time.deltaTime);
        }
    }
}