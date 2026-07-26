using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable {
    [Header("Interaction Settings")]
    [Tooltip("Distância máxima para a interação ocorrer.")]
    [SerializeField] private float interactionDistance = 1f;

    [Tooltip("Ponto específico para onde o player deve andar. Se vazio, usa o transform do objeto.")]
    [SerializeField] private Transform interactionPoint;

    [Header("Visual Feedback")]
    [Tooltip("Componente responsável por ligar/desligar o outline do objeto interagível.")]
    [SerializeField] private InteractableOutline outline;

    public float InteractionDistance => interactionDistance;
    public virtual Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

    public virtual void OnHoverEnter() {
        EnsureOutlineReference();
        if (outline != null) {
            outline.EnableOutline();
        }
    }

    public virtual void OnHoverExit() {
        EnsureOutlineReference();
        if (outline != null) {
            outline.DisableOutline();
        }
    }

    private void EnsureOutlineReference()
    {
        if (outline == null)
        {
            outline = GetComponentInChildren<InteractableOutline>(true);
        }
    }

    public abstract void Interact(GameObject interactor);
}
