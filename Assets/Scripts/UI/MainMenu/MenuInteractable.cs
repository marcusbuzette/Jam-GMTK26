using UnityEngine;
using UnityEngine.Events;

public class MenuInteractable : MonoBehaviour
{
    [Header("Configuracao de Camera")]
    [Tooltip("A Virtual Camera do Cinemachine que focara neste objeto.")]
    public GameObject virtualCamera;

    [Header("Eventos Adicionais (Opcional)")]
    [Tooltip("Use para ativar botoes UI, tocar sons, etc., quando o objeto for clicado.")]
    public UnityEvent onInteract;

    public void Interact()
    {
        // Dispara qualquer evento configurado no Inspector
        onInteract?.Invoke();
    }
}