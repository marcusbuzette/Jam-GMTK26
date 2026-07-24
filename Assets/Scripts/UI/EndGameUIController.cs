using UnityEngine;

public class EndGameUIController : MonoBehaviour {

    [SerializeField] private GameObject failPanel;
    [SerializeField] private GameObject winPanel;


    void Start() {
        failPanel.SetActive(false);
        winPanel.SetActive(false);
        LevelManager.OnLevelVictory += ShowWinPanel;
        LevelManager.OnLevelDefeat += ShowFailPanel;
    }

    private void ShowWinPanel() {
        winPanel.SetActive(true);
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void HandleNextLevel() {
        GameManager.Instance.SelectAndStartLevel(GameManager.Instance.CurrentLevelIndex);
    }

}
