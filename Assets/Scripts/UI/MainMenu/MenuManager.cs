using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiegeticMenuManager : MonoBehaviour
{
    [Header("Configurações de Câmera")]
    [Tooltip("A Virtual Camera do Cinemachine com a visão geral do menu.")]
    [SerializeField] private GameObject defaultMainCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float transitionTime = 2f;

    [Header("Configurações de Interação")]
    [SerializeField] private LayerMask interactableLayer;

    private InputSystem_Actions inputActions;
    private GameObject currentActiveCamera;
    private bool isTransitioning = false;

    private void Awake() 
    {
        // Instancia a mesma classe gerada do Input System que você já usa
        inputActions = new InputSystem_Actions();
        
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable() 
    {
        inputActions.UI.Enable();
        
        inputActions.UI.Click.performed += OnUIClicked;

        inputActions.UI.Cancel.performed += OnCancelClicked;
    }

    private void OnDisable() 
    {
        inputActions.UI.Click.performed -= OnUIClicked;
        inputActions.UI.Disable();
    }

    private void Start() 
    {
        currentActiveCamera = defaultMainCamera;
        currentActiveCamera.SetActive(true);
    }

    private void OnUIClicked(InputAction.CallbackContext context) 
    {
        if (isTransitioning) return;

        // Lê a posição do mouse da mesma forma que no seu PlayerInteract
        Vector2 mousePos = inputActions.UI.Point.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, interactableLayer)) 
        {
            MenuInteractable interactable = hit.collider.GetComponent<MenuInteractable>();
            
            if (interactable != null)
            {
                interactable.Interact();

                if (interactable.virtualCamera != null && interactable.virtualCamera != currentActiveCamera) 
                {
                    SwitchCamera(interactable.virtualCamera);
                }
            }
        }
    }

    // Opcional: Método para ser chamado se você mapear um botão de voltar no Input System
    private void OnCancelClicked(InputAction.CallbackContext context)
    {
        if (currentActiveCamera != defaultMainCamera && !isTransitioning) 
        {
            SwitchCamera(defaultMainCamera);
        }
    }

    public void SwitchCamera(GameObject newCamera) 
    {
        StartCoroutine(TransitionRoutine(newCamera));
    }

    private IEnumerator TransitionRoutine(GameObject newCamera) 
    {
        isTransitioning = true;
        
        currentActiveCamera.SetActive(false);
        newCamera.SetActive(true);
        currentActiveCamera = newCamera;

        yield return new WaitForSeconds(transitionTime);
        
        isTransitioning = false;
    }
}