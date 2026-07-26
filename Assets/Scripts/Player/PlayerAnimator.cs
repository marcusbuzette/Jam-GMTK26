using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAnimator : MonoBehaviour {
    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private PlayerMovement playerMovement;
    private bool isTalking = false;
    private bool isInReaction = false;
    private float currentReactingTimer = 0f;
    private int currentReaction;

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
        if (isTalking) { ConversationReactions(); }
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
            animator.SetFloat(speedHash, normalizedSpeed, 0.1f, Time.deltaTime);
        }
    }

    public void SetTalking(bool _isTalking) {
        if (animator != null) {
            animator.SetBool(isTalkingHash, _isTalking);
            isTalking = _isTalking;
        }
    }

    public void ConversationReactions() {
        Debug.Log("Conversando");
        if (!isInReaction) {
            var reaction = Random.Range(0, 7);

            if (reaction != currentReaction) {
                currentReaction = reaction;
                switch (reaction) {
                    case 0:
                        animator.SetTrigger("Inter_Talking");
                        break;
                    case 1:
                        animator.SetTrigger("Inter_Closed");
                        break;
                    case 2:
                        animator.SetTrigger("Inter_Sad");
                        break;
                    case 3:
                        animator.SetTrigger("Inter_Angry");
                        break;
                    case 4:
                        animator.SetTrigger("Inter_Exclamation");
                        break;
                    case 5:
                        animator.SetTrigger("Inter_Happy");
                        break;
                    case 6:
                        animator.SetTrigger("Inter_Question");
                        break;
                }

                isInReaction = true;
            }
        } else {
            currentReactingTimer += Time.deltaTime;
            Debug.Log("Timer: " + currentReactingTimer);

            if (currentReactingTimer >= 3f) {
                isInReaction = false;
                currentReactingTimer = 0f;
            }
        }
    }
}