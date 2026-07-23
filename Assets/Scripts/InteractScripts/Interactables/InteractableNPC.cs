using UnityEngine;

public class InteractableNPC : InteractableBase {
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Facing")]
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private string activeConversationId;
    private bool shouldReturnToOriginalRotation;
    private bool isTurning;

    private void Awake()
    {
        originalRotation = transform.rotation;
        targetRotation = originalRotation;

        var manager = DialogueManager.Instance;
        if (manager != null)
        {
            manager.DialogueFinished += HandleDialogueFinished;
        }
    }

    private void OnDestroy()
    {
        var manager = DialogueManager.Instance;
        if (manager != null)
        {
            manager.DialogueFinished -= HandleDialogueFinished;
        }
    }

    private void Update()
    {
        if (!isTurning)
        {
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) <= 0.1f)
        {
            transform.rotation = targetRotation;
            isTurning = false;
            Debug.Log($"[NPC] Finished turning. Current rotation: {transform.rotation.eulerAngles}, shouldReturnToOriginalRotation: {shouldReturnToOriginalRotation}");
        }
    }

    public override void Interact(GameObject interactor)
    {
        Debug.Log("Iniciando conversa com o NPC...");
        if (dialogue)
        {
            FaceInteractor(interactor);
            activeConversationId = dialogue.ConversationId;
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("NPC de nome " + gameObject.name + " não tem diálogo configurado.");
        }
    }

    private void FaceInteractor(GameObject interactor)
    {
        if (interactor == null)
        {
            return;
        }

        originalRotation = transform.rotation;
        Debug.Log($"[NPC] FaceInteractor called. Saved originalRotation: {originalRotation.eulerAngles}");

        var directionToInteractor = interactor.transform.position - transform.position;
        directionToInteractor.y = 0f;
        if (directionToInteractor.sqrMagnitude <= 0.001f)
        {
            return;
        }

        targetRotation = Quaternion.LookRotation(directionToInteractor.normalized);
        isTurning = true;
        shouldReturnToOriginalRotation = true;
        Debug.Log($"[NPC] Starting turn to face interactor. targetRotation: {targetRotation.eulerAngles}");
    }

    private void HandleDialogueFinished(Dialogue finishedDialogue)
    {
        Debug.Log($"[NPC] HandleDialogueFinished called. finishedDialogue={finishedDialogue?.ConversationId}, activeConversationId={activeConversationId}, shouldReturn={shouldReturnToOriginalRotation}");
        
        if (finishedDialogue == null || finishedDialogue.ConversationId != activeConversationId)
        {
            Debug.Log($"[NPC] Dialogue mismatch: {finishedDialogue?.ConversationId} != {activeConversationId}");
            return;
        }

        if (!shouldReturnToOriginalRotation)
        {
            Debug.Log($"[NPC] shouldReturnToOriginalRotation is false");
            return;
        }

        Debug.Log($"[NPC] Setting rotation back to original: {originalRotation.eulerAngles}");
        targetRotation = originalRotation;
        isTurning = true;
        shouldReturnToOriginalRotation = false;
    }

}
