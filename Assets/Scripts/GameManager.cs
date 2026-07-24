using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Configuração de Fases")]
    [SerializeField] private LevelSetupSO[] allLevels; // Lista de todas as 10 fases na ordem

    // Armazena a fase selecionada para ser lida pelo LevelManager na cena de jogo
    public LevelSetupSO SelectedLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; } = 0;

    [Header("Nome das Cenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";

    private const string SAVED_LEVEL_KEY = "SavedLevelIndex";

    private void Awake() {
        // Singleton Persistente
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public LevelSetupSO[] GetAllLevels() => allLevels;


    public void SelectAndStartLevel(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= allLevels.Length) return;

        CurrentLevelIndex = levelIndex;
        SelectedLevel = allLevels[levelIndex];

        // Salva o progresso localmente (para o botão "Continuar")
        PlayerPrefs.SetInt(SAVED_LEVEL_KEY, CurrentLevelIndex);
        PlayerPrefs.Save();

        // Carrega a cena do jogo
        SceneManager.LoadScene(gameSceneName);
    }


    public void ContinueGame() {
        int savedIndex = PlayerPrefs.GetInt(SAVED_LEVEL_KEY, 0);
        SelectAndStartLevel(savedIndex);
    }


    public void StartNewGame() {
        SelectAndStartLevel(0);
    }


    public void LoadNextLevel() {
        if (CurrentLevelIndex + 1 < allLevels.Length) {
            SelectAndStartLevel(CurrentLevelIndex + 1);
        } else {
            // Fim do jogo / Zerou a Jam!
            ReturnToMainMenu();
        }
    }

    public void UnlockNextLevel() {
        int nextLevelIndex = CurrentLevelIndex + 1;
        if (nextLevelIndex < allLevels.Length) {
            int savedIndex = PlayerPrefs.GetInt(SAVED_LEVEL_KEY, 0);
            if (nextLevelIndex > savedIndex) {
                PlayerPrefs.SetInt(SAVED_LEVEL_KEY, nextLevelIndex);
                PlayerPrefs.Save();
            }
        }
    }

    public void ReturnToMainMenu() {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsLevelUnlocked(int levelIndex) {
        int savedIndex = PlayerPrefs.GetInt(SAVED_LEVEL_KEY, 0);
        Debug.Log($"Checking if level {levelIndex} is unlocked. Saved index: {savedIndex}");
        return levelIndex <= savedIndex;
    }
}