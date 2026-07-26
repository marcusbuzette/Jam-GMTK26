using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimator : MonoBehaviour {
    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private PlayerMovement playerMovement;

    // Hashes para acesso performático dos parâmetros do Animator
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isTalkingHash = Animator.StringToHash("IsTalking");

    private void Awake() {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update() {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation() {
        float currentSpeed = 0f;

        // 1. Movimentação Manual (CharacterController)
        if (characterController != null && characterController.enabled) {
            Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
            currentSpeed = horizontalVelocity.magnitude;
        }
        // 2. Movimentação Automática / Point and Click (NavMeshAgent)
        else if (navMeshAgent != null && navMeshAgent.enabled) {
            currentSpeed = navMeshAgent.velocity.magnitude;
        }

        // Normaliza a velocidade de 0 a 1 em relação à velocidade máxima de movimento
        float maxSpeed = playerMovement != null ? playerMovement.MoveSpeed : 5f;
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);

        // Atualiza a velocidade no Animator com suavização (damping)
        if (animator != null) {
            Debug.Log(normalizedSpeed);
            animator.SetFloat(speedHash, normalizedSpeed, 0.1f, Time.deltaTime);
        }
    }

    public void SetTalking(bool isTalking) {
        if (animator != null) {
            animator.SetBool(isTalkingHash, isTalking);
        }
    }
}