using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    public SettingsData CurrentSettings { get; private set; } = new SettingsData();

    private string saveFilePath;

    private void Awake() {
        // Padrão Singleton para garantir apenas uma instância entre as cenas
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Define o caminho do arquivo JSON no disco do usuário
        saveFilePath = Path.Combine(Application.persistentDataPath, "settings.json");

        LoadSettings();
    }

    private void OnEnable() {
        // Se inscreve para reaplicar configurações sempre que uma nova cena carregar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Re-aplica áudio e vídeo após carregar a cena
        ApplyAllSettings();
    }

    // --- MÉTODOS DE APLICAÇÃO ---

    public void ApplyAllSettings() {
        SetMasterVolume(CurrentSettings.masterVolume);
        SetMusicVolume(CurrentSettings.musicVolume);
        SetSFXVolume(CurrentSettings.sfxVolume);
        SetFullscreen(CurrentSettings.isFullscreen);

        Resolution[] resolutions = Screen.resolutions;
        if (CurrentSettings.resolutionIndex >= 0 && CurrentSettings.resolutionIndex < resolutions.Length) {
            SetResolution(CurrentSettings.resolutionIndex);
        }
    }

    public void SetMasterVolume(float volume) {
        CurrentSettings.masterVolume = volume;
        if (audioMixer != null)
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    public void SetMusicVolume(float volume) {
        CurrentSettings.musicVolume = volume;
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    public void SetSFXVolume(float volume) {
        CurrentSettings.sfxVolume = volume;
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
    }

    public void SetFullscreen(bool isFullscreen) {
        CurrentSettings.isFullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex) {
        CurrentSettings.resolutionIndex = resolutionIndex;
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length) {
            Resolution res = resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
    }

    // --- SALVAMENTO E CARREGAMENTO EM JSON ---

    public void SaveSettings() {
        string json = JsonUtility.ToJson(CurrentSettings, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Configurações salvas em JSON em: {saveFilePath}");
    }

    public void LoadSettings() {
        if (File.Exists(saveFilePath)) {
            try {
                string json = File.ReadAllText(saveFilePath);
                CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
            } catch {
                CurrentSettings = new SettingsData();
            }
        } else {
            // Arquivo não existe ainda (primeira execução)
            CurrentSettings = new SettingsData();

            // Define a resolução atual nativa como padrão
            Resolution[] resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++) {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height) {
                    CurrentSettings.resolutionIndex = i;
                    break;
                }
            }
            SaveSettings();
        }

        ApplyAllSettings();
    }

    public void GameQuit() {
        Invoke("_GameQuit", 2.5f);
    }

    private void _GameQuit() {
        Application.Quit();
    }
}