using UnityEngine;

public class VinylTimeUI : MonoBehaviour {
    [Header("Referências Visuais")]
    [SerializeField] private RectTransform vinylTransform;
    [SerializeField] private RectTransform needleTransform;

    [Header("Configurações do Vinil")]
    [Tooltip("Velocidade de rotação do disco. Negativo para girar no sentido horário.")]
    [SerializeField] private float vinylSpinSpeed = -150f;

    [Header("Configurações da Agulha (Ângulo Z)")]
    [Tooltip("Posição da agulha quando o tempo está cheio (borda do disco)")]
    [SerializeField] private float needleStartAngle = 0f;
    [Tooltip("Posição da agulha quando o tempo acaba (centro do disco)")]
    [SerializeField] private float needleEndAngle = -45f;

    private float maxTime;
    private bool isPlaying = false;

    private void OnEnable() {
        // Se inscreve nos eventos do LevelManager para manter o código limpo e reativo
        LevelManager.OnLevelStarted += HandleLevelStarted;
        LevelManager.OnTimerUpdated += HandleTimerUpdated;
        LevelManager.OnLevelDefeat += HandleLevelEnded;
        LevelManager.OnLevelVictory += HandleLevelEnded;
    }

    private void OnDisable() {
        // Sempre desinscrever eventos no OnDisable para evitar memory leaks
        LevelManager.OnLevelStarted -= HandleLevelStarted;
        LevelManager.OnTimerUpdated -= HandleTimerUpdated;
        LevelManager.OnLevelDefeat -= HandleLevelEnded;
        LevelManager.OnLevelVictory -= HandleLevelEnded;
    }

    private void HandleLevelStarted() {
        // Pega o tempo inicial da fase para calcular a porcentagem depois
        if (LevelManager.Instance != null) {
            maxTime = LevelManager.Instance.RemainingTime;
            isPlaying = true;
        }
    }

    private void HandleLevelEnded() {
        // Para a rotação do disco quando o jogo acaba
        isPlaying = false;
    }

    private void Update() {
        if (!isPlaying) return;

        // Gira o disco de vinil continuamente no eixo Z
        vinylTransform.Rotate(0f, 0f, vinylSpinSpeed * Time.deltaTime);
    }

    private void HandleTimerUpdated(float remainingTime) {
        if (maxTime <= 0) return;

        // Calcula a porcentagem do tempo restante (vai de 1.0 a 0.0)
        float timePercent = Mathf.Clamp01(remainingTime / maxTime);

        // Interpola o ângulo da agulha com base na porcentagem do tempo
        float currentAngle = Mathf.Lerp(needleEndAngle, needleStartAngle, timePercent);

        // Aplica o ângulo na agulha
        needleTransform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
    }
}
