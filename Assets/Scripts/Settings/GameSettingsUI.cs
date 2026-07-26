using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSettingsUI : MonoBehaviour {
    [Header("Áudio")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Vídeo")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start() {
        SetupResolutions();
        UpdateUIValues();

        // Inscreve os métodos nos eventos da UI
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void SetupResolutions() {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++) {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
    }

    private void UpdateUIValues() {
        if (SettingsManager.Instance == null) return;

        SettingsData data = SettingsManager.Instance.CurrentSettings;

        // Como essa função é chamada no Start ANTES dos AddListener, 
        // usar .value e .isOn não vai disparar os eventos de salvamento indevidamente.
        masterSlider.value = data.masterVolume;
        musicSlider.value = data.musicVolume;
        sfxSlider.value = data.sfxVolume;

        fullscreenToggle.isOn = data.isFullscreen;

        resolutionDropdown.value = data.resolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    // --- MÉTODOS DISPARADOS PELA UI ---

    private void OnMasterChanged(float value) {
        SettingsManager.Instance.SetMasterVolume(value);
        SettingsManager.Instance.SaveSettings();
    }

    private void OnMusicChanged(float value) {
        SettingsManager.Instance.SetMusicVolume(value);
        SettingsManager.Instance.SaveSettings();
    }

    private void OnSFXChanged(float value) {
        SettingsManager.Instance.SetSFXVolume(value);
        SettingsManager.Instance.SaveSettings();
    }

    private void OnFullscreenChanged(bool isFullscreen) {
        SettingsManager.Instance.SetFullscreen(isFullscreen);
        SettingsManager.Instance.SaveSettings();
    }

    private void OnResolutionChanged(int index) {
        SettingsManager.Instance.SetResolution(index);
        SettingsManager.Instance.SaveSettings();
    }
}