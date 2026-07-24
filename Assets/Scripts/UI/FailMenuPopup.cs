using System.Collections;
using UnityEngine;

public class FailMenuPopup : MonoBehaviour {
    [Header("Configurações da Animação")]
    [Tooltip("Tempo total da animação em segundos")]
    public float duration = 0.6f;

    [Tooltip("Quantas oscilações ele faz antes de parar (Vibe de impacto)")]
    public float elasticity = 6f;

    [Tooltip("Quão rápido a oscilação perde a força (Atrito/Frenagem)")]
    public float damping = 5f;

    private Coroutine animationCoroutine;

    private void OnEnable() {
        // Se já houver uma animação rodando (ex: ativou e desativou rápido), paramos ela.
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimatePopupRoutine());
    }

    private IEnumerator AnimatePopupRoutine() {
        float timeElapsed = 0f;
        transform.localScale = Vector3.zero;

        while (timeElapsed < duration) {
            timeElapsed += Time.deltaTime;

            // t é o progresso normalizado da animação de 0 até 1
            float t = timeElapsed / duration;

            // 1. CRESCIMENTO EXPLOSIVO (Ease Out Exponencial)
            // A curva sobe muito rápido no início e desacelera no final
            float baseScale = 1f - Mathf.Pow(2f, -10f * t);

            // 2. A ONDA DO IMPACTO (Oscilador Amortecido)
            // Seno cria o vai-e-vem, Exp negativo atua como um "freio" que reduz a onda a zero.
            float wobble = Mathf.Sin(t * Mathf.PI * elasticity) * Mathf.Exp(-damping * t);

            // 3. SQUASH & STRETCH (Volume Constante)
            // Para parecer orgânico, quando o Y estica, o X esmaga na mesma proporção.
            float scaleY = baseScale + (wobble * 1.5f); // Y sofre o impacto direto da explosão de baixo pra cima
            float scaleX = baseScale - (wobble * 0.8f); // X compensa o volume retraindo

            transform.localScale = new Vector3(scaleX, scaleY, 1f);

            // Espera até o próximo frame
            yield return null;
        }

        // Garante que a escala termine perfeitamente em (1, 1, 1) ao final do tempo
        transform.localScale = Vector3.one;
    }
}
