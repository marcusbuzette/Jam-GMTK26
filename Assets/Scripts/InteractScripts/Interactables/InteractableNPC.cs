using UnityEngine;

public class InteractableNPC : InteractableBase {
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Header("Facing")]
    [SerializeField, Min(0f)] private float turnSpeed = 360f;

    [Header("Outline References")]
    [SerializeField] private InteractableOutline maleOutline;
    [SerializeField] private InteractableOutline femaleOutline;

    private Quaternion originalRotation;
    private Quaternion targetRotation;
    private string activeConversationId;
    private bool shouldReturnToOriginalRotation;
    private bool isTurning;
    private NpcAppearanceIdentity appearanceIdentity;

    private void Awake()
    {
        originalRotation = transform.rotation;
        targetRotation = originalRotation;
        appearanceIdentity = GetComponent<NpcAppearanceIdentity>();

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
        }
    }

    public override void Interact(GameObject interactor)
    {
        Debug.Log("Iniciando conversa com o NPC...");
        if (dialogue)
        {
            FaceInteractor(interactor);
            activeConversationId = dialogue.ConversationId;
            DialogueManager.Instance.StartDialogue(dialogue, appearanceIdentity);
        }
        else
        {
            Debug.LogWarning("NPC de nome " + gameObject.name + " não tem diálogo configurado.");
        }
    }

    public override void OnHoverEnter()
    {
        var outline = GetActiveOutline();
        if (outline != null)
        {
            outline.EnableOutline();
            return;
        }

        base.OnHoverEnter();
    }

    public override void OnHoverExit()
    {
        var outline = GetActiveOutline();
        if (outline != null)
        {
            outline.DisableOutline();
            return;
        }

        base.OnHoverExit();
    }

    private InteractableOutline GetActiveOutline()
    {
        if (appearanceIdentity != null && appearanceIdentity.HasAppearance)
        {
            return appearanceIdentity.CurrentAppearance.bodyType == NpcBodyType.Female
                ? femaleOutline ?? maleOutline
                : maleOutline ?? femaleOutline;
        }

        return maleOutline ?? femaleOutline;
    }

    private void FaceInteractor(GameObject interactor)
    {
        if (interactor == null)
        {
            return;
        }

        originalRotation = transform.rotation;

        var directionToInteractor = interactor.transform.position - transform.position;
        directionToInteractor.y = 0f;
        if (directionToInteractor.sqrMagnitude <= 0.001f)
        {
            return;
        }

        targetRotation = Quaternion.LookRotation(directionToInteractor.normalized);
        isTurning = true;
        shouldReturnToOriginalRotation = true;
    }

    private void HandleDialogueFinished(Dialogue finishedDialogue)
    {
        
        if (finishedDialogue == null || finishedDialogue.ConversationId != activeConversationId)
        {
            Debug.Log($"[NPC] Dialogue mismatch: {finishedDialogue?.ConversationId} != {activeConversationId}");
            return;
        }

        if (!shouldReturnToOriginalRotation)
        {
            return;
        }
        targetRotation = originalRotation;
        isTurning = true;
        shouldReturnToOriginalRotation = false;
    }

}
