using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable {
    [Header("Interaction Settings")]
    [Tooltip("Distância máxima para a interação ocorrer.")]
    [SerializeField] private float interactionDistance = 1f;

    [Tooltip("Ponto específico para onde o player deve andar. Se vazio, usa o transform do objeto.")]
    [SerializeField] private Transform interactionPoint;

    [Header("Visual Feedback")]
    [Tooltip("Coloque aqui o componente de Outline (ou outro script de feedback visual).")]
    [SerializeField] private InteractableOutline[] outlineComponents;

    public float InteractionDistance => interactionDistance;
    public virtual Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

    public virtual void OnHoverEnter() {
        for (int i = 0; i < outlineComponents.Length; i++) {
            if (outlineComponents[i] != null) {
                outlineComponents[i].EnableOutline();
            }
        }
    }

    public virtual void OnHoverExit() {
        for (int i = 0; i < outlineComponents.Length; i++) {
            if (outlineComponents[i] != null) {
                outlineComponents[i].DisableOutline();
            }
        }
    }

    public abstract void Interact(GameObject interactor);
}
