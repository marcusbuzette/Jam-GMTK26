using UnityEngine;
using UnityEngine.Events;

public class MenuInteractable : InteractableBase {
    [Header("Configuração de Câmera do Menu")]
    [Tooltip("A Virtual Camera do Cinemachine que focará neste objeto.")]
    public GameObject virtualCamera;

    [Header("Eventos Adicionais do Menu")]
    [Tooltip("Ative botões de um Canvas, inicie animações ou sons ao clicar.")]
    public UnityEvent onInteract;

    [Tooltip("Chamado quando o jogador clica em outro objeto ou aperta ESC.")]
    public UnityEvent onExit;

    bool canInteract = true;

    [SerializeField] private InteractableOutline outlineComponentSelected;

    [SerializeField] private Color onSelectedColor;

    void Start() {
        SetCanInteract(true);
    }

    public override void Interact(GameObject interactor) {
        onInteract?.Invoke();
    }

    public void Exit() {
        onExit?.Invoke();
    }

    public override void OnHoverEnter() {
        if (outlineComponentSelected != null) outlineComponentSelected.ChangeColor(onSelectedColor);
    }

    public override void OnHoverExit() {
        if (outlineComponentSelected != null) outlineComponentSelected.ChangeColor(Color.white);
    }

    public void SetCanInteract(bool value) {
        canInteract = value;

        if (canInteract) {
            if (outlineComponentSelected != null) outlineComponentSelected.EnableOutline();
        } else {
            if (outlineComponentSelected != null) outlineComponentSelected.DisableOutline();
        }
    }

    public bool GetCanInteract() => canInteract;
}