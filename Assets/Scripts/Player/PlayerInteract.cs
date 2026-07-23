using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterController))]
public class PlayerInteract : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera mainCamera;

    private InputSystem_Actions inputActions;
    private NavMeshAgent agent;
    private CharacterController characterController;
    private PlayerMovement playerMovementScript;

    private IInteractable currentHovered;
    private IInteractable targetInteractable;

    private enum AutoActionState { None, NavigatingToInteract, Aligning, NavigatingToPoint }
    private AutoActionState currentState = AutoActionState.None;

    private bool isNavigatingToInteract;
    private bool isAligningToInteract;

    private void Awake() {
        inputActions = new InputSystem_Actions();
        agent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        playerMovementScript = GetComponent<PlayerMovement>();

        // Desliga a rotação automática para aplicarmos a nossa lógica
        agent.updateRotation = false;

        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable() {
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += OnInteractClicked;
        inputActions.UI.Enable();
    }

    private void OnDisable() {
        inputActions.Player.Disable();
        inputActions.Player.Interact.performed -= OnInteractClicked;
        inputActions.UI.Disable();
    }

    private void Update() {
        HandleHover();
        
        if (isNavigatingToInteract) {
            HandleNavMeshMovement();
        } else if (isAligningToInteract) {
            HandleAlignment();
        }

        CheckManualMovementOverride();
    }

    private void HandleHover() {
        Vector2 mousePos = inputActions.UI.Point.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, interactableLayer)) {
            IInteractable interactable = hit.collider.GetComponentInChildren<IInteractable>();
            if (interactable != null) {
                if (currentHovered != interactable) {
                    currentHovered?.OnHoverExit();
                    currentHovered = interactable;
                    currentHovered.OnHoverEnter();
                }
                return;
            }
        }

        if (currentHovered != null) {
            currentHovered.OnHoverExit();
            currentHovered = null;
        }
    }

    private void OnInteractClicked(InputAction.CallbackContext context) {
        Vector2 mousePos = inputActions.UI.Point.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // Prioridade 1: Clicou em algo interativo
        if (currentHovered != null) {
            targetInteractable = currentHovered;
            float distance = Vector3.Distance(transform.position, targetInteractable.InteractionPoint.position);

            if (distance <= targetInteractable.InteractionDistance) {
                StartAlignment();
            } else {
                MoveToInteractionPoint();
            }
            return;
        }

        // Prioridade 2: Clicou no chão (Point and click livre)
        if (Physics.Raycast(ray, out RaycastHit groundHit, 10000f, groundLayer)) {
            // Valida se o ponto atingido realmente está dentro da área navegável do NavMesh
            if (NavMesh.SamplePosition(groundHit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas)) {
                MoveToTarget(navHit.position, 0.1f, AutoActionState.NavigatingToPoint);
            }
        }
    }

    private void MoveToTarget(Vector3 destination, float stopDistance, AutoActionState newState) {
        playerMovementScript.enabled = false;
        characterController.enabled = false;

        agent.enabled = true;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(destination);
        
        currentState = newState;

        isNavigatingToInteract = true;
        isAligningToInteract = false;
    }

    private void MoveToInteractionPoint() {
        playerMovementScript.enabled = false;
        characterController.enabled = false;

        agent.enabled = true;
        agent.stoppingDistance = targetInteractable.InteractionDistance;
        agent.SetDestination(targetInteractable.InteractionPoint.position);

        isNavigatingToInteract = true;
        isAligningToInteract = false;
    }

    private void StartAlignment() {
        playerMovementScript.enabled = false;
        characterController.enabled = false;
        
        if (agent.enabled) agent.isStopped = true;

        isNavigatingToInteract = false;
        isAligningToInteract = true;
    }

    private void HandleNavMeshMovement() {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance) {
            StartAlignment();
            return;
        }

        Vector3 targetDir = agent.steeringTarget - transform.position;
        targetDir.y = 0;

        if (targetDir.sqrMagnitude > 0.01f) {
            targetDir.Normalize();
            
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, playerMovementScript.RotationSpeed * Time.deltaTime);

            float angle = Vector3.Angle(transform.forward, targetDir);
            agent.isStopped = (angle > playerMovementScript.AngleToStartMoving);
        }
    }

    private void HandleAlignment() {
        if (targetInteractable == null) {
            CancelAutoInteraction();
            return;
        }

        Transform targetTransform = (targetInteractable as MonoBehaviour)?.transform ?? targetInteractable.InteractionPoint;
        
        Vector3 dirToTarget = targetTransform.position - transform.position;
        dirToTarget.y = 0;

        if (dirToTarget.sqrMagnitude > 0.01f) {
            dirToTarget.Normalize();
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, playerMovementScript.RotationSpeed * Time.deltaTime);

            if (Vector3.Angle(transform.forward, dirToTarget) <= 5f) {
                ExecuteInteraction();
            }
        } else {
            ExecuteInteraction();
        }
    }

    private void ExecuteInteraction() {
        targetInteractable.Interact(gameObject);
        CancelAutoInteraction();
    }

    private void CheckManualMovementOverride() {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.1f && (isNavigatingToInteract || isAligningToInteract)) {
            CancelAutoInteraction();
        }
    }

    private void CancelAutoInteraction() {
        targetInteractable = null;
        isNavigatingToInteract = false;
        isAligningToInteract = false;
        
        if (agent.enabled && agent.isOnNavMesh) {
            agent.ResetPath();
        }
        
        RestoreManualMovement();
    }

    private void RestoreManualMovement() {
        agent.enabled = false;
        characterController.enabled = true;
        playerMovementScript.enabled = true;
    }
}