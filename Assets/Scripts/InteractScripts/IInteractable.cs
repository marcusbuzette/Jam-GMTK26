using UnityEngine;

public interface IInteractable
{
    float InteractionDistance { get; }
    Transform InteractionPoint { get; }
    void OnHoverEnter();
    void OnHoverExit();
    void Interact(GameObject interactor);
}
