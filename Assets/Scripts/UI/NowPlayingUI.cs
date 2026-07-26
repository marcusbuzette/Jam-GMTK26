using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NowPlayingUI : MonoBehaviour {
    public static NowPlayingUI Instance { get; private set; }

    [Header("Referências da UI")]
    [SerializeField] private CanvasGroup uiGroup;
    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI rpmText;
    [SerializeField] private Slider progressSlider;

    [Header("Textos de Tempo")]
    [SerializeField] private TextMeshProUGUI currentTimeText; // Ex: 0:45
    [SerializeField] private TextMeshProUGUI totalTimeText;   // Ex: 3:12

    private AudioSource currentAudioSource;
    private bool isShowing = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        uiGroup.alpha = 0f;
        uiGroup.interactable = false;
        uiGroup.blocksRaycasts = false;
    }

    public void ToggleNowPlaying(AudioSource source, string songName, int rpm) {
        isShowing = !isShowing;

        if (isShowing) {
            currentAudioSource = source;
            songNameText.text = songName;
            rpmText.text = $"{rpm} RPM";

            // Configura o valor máximo do slider com base na duração do áudio
            float totalSeconds = source.clip.length;
            progressSlider.maxValue = totalSeconds;

            // Formata e exibe o tempo total da música
            totalTimeText.text = FormatTime(totalSeconds);

            ShowUI();
        } else {
            currentAudioSource = null;
            HideUI();
        }
    }

    private void Update() {
        if (isShowing && currentAudioSource != null && currentAudioSource.isPlaying) {
            // Atualiza a posição do slider
            progressSlider.value = currentAudioSource.time;

            // Formata e exibe o tempo atual em tempo real
            currentTimeText.text = FormatTime(currentAudioSource.time);
        }
    }

    // Método auxiliar para formatar os segundos em "0:00"
    private string FormatTime(float timeInSeconds) {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        // O "00" garante que os segundos sempre tenham 2 dígitos (ex: 1:05 em vez de 1:5)
        return string.Format("{0}:{1:00}", minutes, seconds);
    }

    private void ShowUI() {
        uiGroup.alpha = 1f;
    }

    private void HideUI() {
        uiGroup.alpha = 0f;
    }
}