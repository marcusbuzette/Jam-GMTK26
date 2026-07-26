using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ManualManager : MonoBehaviour {
    public static ManualManager Instance { get; private set; }
    [Header("Dados e Referências")]
    [SerializeField] private ManualDataSO manualData;
    [SerializeField] private Transform pageContainer; // Onde o prefab da página será instanciado

    [Header("Botões de Paginação")]
    [SerializeField] private Button btnNext;
    [SerializeField] private Button btnPrevious;
    [SerializeField] private Button btnClose;

    [Header("Configurações de Animação")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]private RectTransform rectTransformWrapper;
    private Vector2 offScreenPosition;
    private Vector2 onScreenPosition;

    private int currentPageIndex = 0;
    private GameObject currentPageInstance;
    private Coroutine slideCoroutine;

    public bool IsOpen { get; private set; } = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        // Define as posições baseadas na largura da tela para o Slide In (da esquerda)
        onScreenPosition = rectTransformWrapper.anchoredPosition;
        offScreenPosition = new Vector2(-Screen.width * 1.5f, onScreenPosition.y);

       
    }

    void Start() {
         // Esconde o painel no início
        rectTransformWrapper.anchoredPosition = offScreenPosition;
        rectTransformWrapper.gameObject.SetActive(false);

        // Configura os botões
        btnNext.onClick.AddListener(NextPage);
        btnPrevious.onClick.AddListener(PreviousPage);
        btnClose.onClick.AddListener(CloseManual);
    }

    private void Update() {
        // Exemplo de atalho para abrir/fechar (pode substituir pelo Input System se preferir)
        if (Keyboard.current.mKey.wasPressedThisFrame) {
            if (IsOpen) CloseManual();
            else OpenManual();
        }
    }

    public void OpenManual() {
        // Validação genérica para impedir que abra por cima de outras coisas (ex: Menu de Pause)
        // Se você tiver um UIManager global, pode checar aqui: if (UIManager.HasActivePanels) return;

        if (IsOpen || manualData == null || manualData.pagePrefabs.Length == 0) return;
        LevelManager.Instance.HandleOpenedPannel();

        IsOpen = true;
        currentPageIndex = 0;
        LoadPage(currentPageIndex);
        UpdateButtonsState();

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(onScreenPosition));
    }

    public void CloseManual() {
        if (!IsOpen) return;

        IsOpen = false;
        LevelManager.Instance.HandleClosedPannel();
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(offScreenPosition, onComplete: ClearCurrentPage));
    }

    private void LoadPage(int index) {
        ClearCurrentPage();

        if (index >= 0 && index < manualData.pagePrefabs.Length) {
            // Instancia a página atual como filha do container
            currentPageInstance = Instantiate(manualData.pagePrefabs[index], pageContainer);

            // Garante que o RectTransform do prefab ocupe 100% do container
            RectTransform pageRect = currentPageInstance.GetComponent<RectTransform>();
            if (pageRect != null) {
                pageRect.anchorMin = Vector2.zero;
                pageRect.anchorMax = Vector2.one;
                pageRect.offsetMin = Vector2.zero;
                pageRect.offsetMax = Vector2.zero;
            }
        }
    }

    private void ClearCurrentPage() {
        if (currentPageInstance != null) {
            Destroy(currentPageInstance);
            currentPageInstance = null;
        }
    }

    private void NextPage() {
        if (currentPageIndex < manualData.pagePrefabs.Length - 1) {
            currentPageIndex++;
            LoadPage(currentPageIndex);
            UpdateButtonsState();
        }
    }

    private void PreviousPage() {
        if (currentPageIndex > 0) {
            currentPageIndex--;
            LoadPage(currentPageIndex);
            UpdateButtonsState();
        }
    }

    private void UpdateButtonsState() {
        // Desativa os botões nos extremos para não loopar
        btnPrevious.interactable = (currentPageIndex > 0);
        btnNext.interactable = (currentPageIndex < manualData.pagePrefabs.Length - 1);
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition, System.Action onComplete = null) {
        Vector2 startPosition = rectTransformWrapper.anchoredPosition;
        float elapsedTime = 0f;
        if (IsOpen) {
            rectTransformWrapper.gameObject.SetActive(true);
        }

        while (elapsedTime < animationDuration) {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / animationDuration;
            float curvePercent = slideCurve.Evaluate(percent);

            rectTransformWrapper.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curvePercent);
            yield return null;
        }

        rectTransformWrapper.anchoredPosition = targetPosition;
        if (!IsOpen) {
            rectTransformWrapper.gameObject.SetActive(false);
        }
        onComplete?.Invoke();
    }

    public void ToggleManual() {
        if (IsOpen) CloseManual();
        else OpenManual();
    }
}