using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine; // Use "using Cinemachine;" se estiver em versões anteriores do Cinemachine

public class CameraZoom : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private CinemachineCamera virtualCamera; // Ou CinemachineVirtualCamera em versões legadas
    [SerializeField] private InputActionReference zoomAction;

    [Header("Configurações de Zoom")]
    [SerializeField] private float minOrthographicSize = 2f;  // Zoom máximo (mais perto)
    [SerializeField] private float maxOrthographicSize = 8f; // Zoom mínimo (mais longe)
    [SerializeField] private float zoomSensitivity = 0.005f;  // Sensibilidade do scroll
    [SerializeField] private float zoomSmoothness = 10f;     // Suavização do movimento (Lerp)

    private float targetSize;

    private void OnEnable()
    {
        if (zoomAction != null)
            zoomAction.action.Enable();
    }

    private void OnDisable()
    {
        if (zoomAction != null)
            zoomAction.action.Disable();
    }

    private void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineCamera>();

        // Inicializa o alvo com o tamanho atual da câmera
        targetSize = virtualCamera.Lens.OrthographicSize;
    }

    private void Update()
    {
        HandleZoomInput();
        ApplySmoothZoom();
    }

    private void HandleZoomInput()
    {
        // Lê o valor Vector2 do scroll do mouse (o eixo Y guarda a rolagem vertical)
        Vector2 scrollValue = zoomAction.action.ReadValue<Vector2>();

        if (scrollValue.y != 0)
        {
            // Se rolar para cima (y positivo), o tamanho diminui (zoom in)
            // Se rolar para baixo (y negativo), o tamanho aumenta (zoom out)
            targetSize -= scrollValue.y * zoomSensitivity;

            // Garante que o zoom fique dentro dos limites mínimos e máximos
            targetSize = Mathf.Clamp(targetSize, minOrthographicSize, maxOrthographicSize);
        }
    }

    private void ApplySmoothZoom()
    {
        // Interpola suavemente o valor atual até o valor alvo (evita zoom "seco")
        float currentSize = virtualCamera.Lens.OrthographicSize;
        virtualCamera.Lens.OrthographicSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * zoomSmoothness);
    }
}