using System.Collections;
using TMPro;
using UnityEngine;

public enum InteractionType {
    Simple,
    PopUp,
    None
}

public class InteractableItem : InteractableBase {

    public InteractionType interactionType;

    [Header("Configurações do Balão (Simple)")]
    [SerializeField] private string textToShow;
    [SerializeField] private Canvas worldSpaceCanvas; // Guarda o balão instanciado para não criar cópias infinitas
    
    [Header("Tempos da Animação (Simple)")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float driftUpDistance = 0.5f;



    [Header("Configurações do PopUp (PopUp)")]
    [SerializeField] private GameObject contentPrefab; // Prefab do conteúdo a ser exibido no painel de interação

    // Referências internas
    private Coroutine simpleInteractionCoroutine;

    public override void Interact(GameObject interactor) {
        if (interactionType == InteractionType.Simple) {
            SimpleInteraction();
        } else if (interactionType == InteractionType.PopUp) {
            PopUpInteraction(interactor);
        }
    }

    private void SimpleInteraction() {
        // Se o jogador clicar várias vezes, paramos a animação atual e reiniciamos
        if (simpleInteractionCoroutine != null) {
            StopCoroutine(simpleInteractionCoroutine);
        }
        simpleInteractionCoroutine = StartCoroutine(SimpleBubbleRoutine());
    }

    private IEnumerator SimpleBubbleRoutine() {;
        worldSpaceCanvas.gameObject.SetActive(true);
        CanvasGroup canvasGroup = worldSpaceCanvas.GetComponent<CanvasGroup>();
        Transform bubbleTransform = worldSpaceCanvas.transform;
        worldSpaceCanvas.GetComponentInChildren<TMP_Text>().text = textToShow;
        // Posição base do balão (fixa em relação ao item)
        Vector3 basePosition = bubbleTransform.position;
        bubbleTransform.localScale = Vector3.one * 0.8f;
        canvasGroup.alpha = 0f;

        float timeElapsed = 0f;

        // --- FASE 1: FADE IN E CRESCIMENTO (Ease Out Sine) ---
        while (timeElapsed < fadeInDuration) {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fadeInDuration;
            float easeCurve = Mathf.Sin(t * Mathf.PI * 0.5f);

            canvasGroup.alpha = easeCurve;
            float currentScale = Mathf.Lerp(0.8f, 1f, easeCurve);
            bubbleTransform.localScale = new Vector3(currentScale, currentScale, 1f);

            yield return null;
        }

        // Garante os valores finais
        canvasGroup.alpha = 1f;
        bubbleTransform.localScale = Vector3.one;

        // --- FASE 2: TEMPO DE LEITURA ---
        yield return new WaitForSeconds(displayDuration);

        // --- FASE 3: FADE OUT E FLUTUAÇÃO (Ease In Sine) ---
        timeElapsed = 0f;
        while (timeElapsed < fadeOutDuration) {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fadeOutDuration;
            float easeCurve = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            canvasGroup.alpha = 1f - easeCurve;
            
            // Faz o balão subir a partir da posição base
            float driftY = Mathf.Lerp(basePosition.y, basePosition.y + driftUpDistance, easeCurve);
            bubbleTransform.position = new Vector3(basePosition.x, driftY, basePosition.z);

            yield return null;
        }

        // Desativa no final para não renderizar à toa
        worldSpaceCanvas.gameObject.SetActive(false);
    }

    private void PopUpInteraction(GameObject interactor) {
        if (InteractPannelController.Instance != null) {
            InteractPannelController.Instance.ShowPannel(contentPrefab);
        } else {
            Debug.LogWarning("InteractPannelController não encontrado na cena.");
        }
    }
}