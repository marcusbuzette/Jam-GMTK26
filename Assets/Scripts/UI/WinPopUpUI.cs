using System.Collections;
using UnityEngine;

public class WinPopUpUI : MonoBehaviour {
    [Header("Configurações da Animação")]
    [Tooltip("Tempo total da animação em segundos")]
    public float duration = 0.8f;

    [Tooltip("Frequência do pulo e da vibração")]
    public float elasticity = 5f;

    [Tooltip("Frenagem (Quanto maior, mais rápido ele estabiliza no centro)")]
    public float damping = 6f;

    [Header("Movimento e Distorção")]
    [Tooltip("Distância no eixo Y de onde o popup vai sair (ex: -1000 para sair de baixo da tela)")]
    public float startYOffset = -1000f;

    [Tooltip("Intensidade do Squash e Stretch baseado na velocidade do pulo")]
    public float stretchIntensity = 0.6f;

    private Coroutine animationCoroutine;
    private Vector3 targetPosition; // A posição final (geralmente o centro da tela onde ele foi posicionado no Editor)

    private void Awake() {
        // Salva a posição original para saber onde é a "linha de chegada"
        targetPosition = transform.localPosition;
    }

    private void OnEnable() {
        if (animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateVictoryRoutine());
    }

    private IEnumerator AnimateVictoryRoutine() {
        float timeElapsed = 0f;

        // Joga o popup para baixo antes de começar
        transform.localPosition = targetPosition + new Vector3(0f, startYOffset, 0f);

        while (timeElapsed < duration) {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;

            // 1. O PULO (Movimento com Overshoot / Follow Through)
            // A curva cosseno invertida faz com que ele passe do alvo (centro) e depois volte
            float springProgress = 1f - Mathf.Cos(t * Mathf.PI * elasticity) * Mathf.Exp(-damping * t);

            // Aplica o progresso na posição Y
            float currentY = Mathf.Lerp(targetPosition.y + startYOffset, targetPosition.y, springProgress);
            transform.localPosition = new Vector3(targetPosition.x, currentY, targetPosition.z);

            // 2. SQUASH & STRETCH (Sincronizado com a "Velocidade")
            // A derivada (velocidade) da nossa equação de movimento é essencialmente o Seno.
            // Quando a velocidade é alta para cima, o valor é positivo (Estica). 
            // Quando ele passa do ponto e cai, o valor é negativo (Esmaga).
            float velocityWobble = Mathf.Sin(t * Mathf.PI * elasticity) * Mathf.Exp(-damping * t);

            // Aplica a deformação preservando o volume visual (se Y cresce, X diminui)
            float scaleY = 1f + (velocityWobble * stretchIntensity);
            float scaleX = 1f - (velocityWobble * stretchIntensity * 0.75f);

            transform.localScale = new Vector3(scaleX, scaleY, 1f);

            yield return null;
        }

        // Garante que o popup termine perfeitamente alinhado e sem distorções
        transform.localPosition = targetPosition;
        transform.localScale = Vector3.one;
    }
}
