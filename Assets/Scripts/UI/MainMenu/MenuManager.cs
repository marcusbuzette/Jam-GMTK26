using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiegeticMenuManager : MonoBehaviour {
    [Header("Configurações de Câmera")]
    [SerializeField] private GameObject defaultMainCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float transitionTime = 2f;

    [Header("Configurações de Interação")]
    [SerializeField] private LayerMask interactableLayer;

    private InputSystem_Actions inputActions;
    private GameObject currentActiveCamera;
    private bool isTransitioning = false;

    private IInteractable currentHovered;

    private MenuInteractable currentActiveInteractable;

    private void Awake() {
        inputActions = new InputSystem_Actions();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void OnEnable() {
        inputActions.UI.Enable();
        inputActions.UI.Click.performed += OnUIClicked;
        inputActions.UI.Cancel.performed += OnCancelClicked;
    }

    private void OnDisable() {
        inputActions.UI.Click.performed -= OnUIClicked;
        inputActions.UI.Cancel.performed -= OnCancelClicked;
        inputActions.UI.Disable();
    }

    private void Start() {
        currentActiveCamera = defaultMainCamera;
        currentActiveCamera.SetActive(true);
    }

    private void Update() {
        if (!isTransitioning) {
            HandleHover();
        }
    }

    private void HandleHover() {
        Vector2 mousePos = inputActions.UI.Point.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, interactableLayer)) {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable is MenuInteractable menuInteractable) {
                if (menuInteractable.GetCanInteract() == false) {
                    if (currentHovered != null) {
                        currentHovered.OnHoverExit();
                        currentHovered = null;
                    }
                    return;
                }
            }

            if (interactable != null) {
                if (currentHovered != interactable) {
                    currentHovered?.OnHoverExit();
                    currentHovered = interactable;
                    currentHovered.OnHoverEnter();
                }
                return;
            }
        }

        if (currentHovered != null) {
            currentHovered.OnHoverExit();
            currentHovered = null;
        }
    }

    private void OnUIClicked(InputAction.CallbackContext context) {
        if (isTransitioning) return;

        if (currentHovered != null && currentHovered is MenuInteractable clickedInteractable) {

            if (clickedInteractable.virtualCamera != null && clickedInteractable.virtualCamera != currentActiveCamera) {

                // Chama o Exit de onde estávamos ANTES de ir para o novo
                if (currentActiveInteractable != null) {
                    currentActiveInteractable.Exit();
                }

                // Chama o Interact do local novo
                clickedInteractable.Interact(gameObject);

                // O novo local passa a ser o ativo
                currentActiveInteractable = clickedInteractable;

                // Inicia a transição da câmera
                SwitchCamera(clickedInteractable.virtualCamera);
            } else {
                // Se não tem câmera nova, só interage normalmente
                clickedInteractable.Interact(gameObject);
            }
        }
    }

    private void OnCancelClicked(InputAction.CallbackContext context) {
        if (currentActiveCamera != defaultMainCamera && !isTransitioning) {

            // Chama o Exit de onde estávamos ao voltar para a visão principal
            if (currentActiveInteractable != null) {
                currentActiveInteractable.Exit();
                currentActiveInteractable = null; // Limpa porque voltamos pra visão geral
            }

            SwitchCamera(defaultMainCamera);
        }
    }

    public void SwitchCamera(GameObject newCamera) {
        StartCoroutine(TransitionRoutine(newCamera));
    }

    private IEnumerator TransitionRoutine(GameObject newCamera) {
        isTransitioning = true;

        // Limpa visualmente o hover de onde o mouse estava clicado
        if (currentHovered != null) {
            currentHovered.OnHoverExit();
            currentHovered = null;
        }

        currentActiveCamera.SetActive(false);
        newCamera.SetActive(true);
        currentActiveCamera = newCamera;

        yield return new WaitForSeconds(transitionTime);

        isTransitioning = false;
    }
}