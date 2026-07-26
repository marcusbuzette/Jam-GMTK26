using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionUI : MonoBehaviour {
    [Header("Referências de UI")]
    [SerializeField] private LevelButton[] levelButtons; // Botoes de fase na ordem

    private void Start() {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons() {
        if (GameManager.Instance == null) return;

        LevelSetupSO[] levels = GameManager.Instance.GetAllLevels();

        for (int i = 0; i < levels.Length; i++) {
            int levelIndex = i; // Copia local da variável para o callback da lambda
            GameObject btnObj = levelButtons[levelIndex].gameObject;

            // Configura o evento do clique
            Button btn = levelButtons[levelIndex].levelButton;
            if (btn != null) {
                btn.onClick.AddListener(() => GameManager.Instance.SelectAndStartLevel(levelIndex));
            }

            LevelButton levelButtonComponent = levelButtons[levelIndex];
            if (levelButtonComponent != null) {
                levelButtonComponent.SetLevelIndex(levelIndex);
                levelButtonComponent.SetLevelButton(GameManager.Instance.IsLevelUnlocked(levelIndex), levels[levelIndex].levelName, levels[levelIndex].levelImage, levels);
            }
        }
    }
}