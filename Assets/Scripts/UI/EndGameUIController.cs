using UnityEngine;

public class EndGameUIController : MonoBehaviour {

    public static EndGameUIController Instance { get; private set; }

    [SerializeField] private GameObject failPanel;
    [SerializeField] private GameObject winPanel;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        failPanel.SetActive(false);
        winPanel.SetActive(false);
        LevelManager.OnLevelVictory += ShowWinPanel;
        LevelManager.OnLevelDefeat += ShowFailPanel;
    }

    private void ShowWinPanel() {
        winPanel.SetActive(true);
        LevelManager.Instance.HandleOpenedPannel(); // Desabilita movimento e interação do player
    }
    
    private void ShowFailPanel() {
        failPanel.SetActive(true);
    }


    private void OnDestroy() {
        LevelManager.OnLevelVictory -= ShowWinPanel;
        LevelManager.OnLevelDefeat -= ShowFailPanel;
    }

    public void HandleBackToMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void HandleRestartLevel() {
        GameManager.Instance.RestartCurrentLevel();
        failPanel.SetActive(false);
        LevelManager.Instance.HandleClosedPannel();
    }

    public void HandleNextLevel() {
        GameManager.Instance.SelectAndStartLevel(GameManager.Instance.CurrentLevelIndex);
    }

    public bool IsEndGameUIActive {
        get {
            return failPanel.activeSelf || winPanel.activeSelf;
        }
    }

}
