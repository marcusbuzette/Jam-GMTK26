using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour {
    private Button button;
    private int levelIndex;
    

    private void Awake() {
        button = GetComponent<Button>();
    }

    private void Start() {
        if (GameManager.Instance != null) {
            button.interactable = GameManager.Instance.IsLevelUnlocked(levelIndex);
        }
    }


    public void SetLevelIndex(int index) {
        levelIndex = index;
    }


    public void SetInteractable(bool isInteractable) {
        button.interactable = isInteractable;
    }
}
