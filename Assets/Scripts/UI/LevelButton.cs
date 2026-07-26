using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour {
    private Button button;
    private int levelIndex;

    public TMP_Text levelNum;
    public TMP_Text levelName;
    public Transform levelLock;
    public Image levelImage;
    public GameObject[] levelStatus;
    public Button levelButton;


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
        index++;
        levelNum.text = index.ToString();
    }


    public void SetLevelButton(bool isInteractable, string _levelName, Sprite _levelImage, LevelSetupSO[] levels) {
        button.interactable = isInteractable;

        levelName.text = _levelName;
        levelImage.sprite = _levelImage;

        levelStatus[0].gameObject.SetActive(false);
        levelStatus[1].gameObject.SetActive(false);
        levelStatus[2].gameObject.SetActive(false);

        if (GameManager.Instance.IsLevelUnlocked(levelIndex)) {
            levelNum.gameObject.SetActive(true);
            levelLock.gameObject.SetActive(false);

            if (levelIndex != levels.Length) {
                if (!GameManager.Instance.IsLevelUnlocked(levelIndex++)) {
                    //level unlock
                    levelStatus[1].gameObject.SetActive(true);
                } else {
                    //level complete
                    levelStatus[2].gameObject.SetActive(true);
                }
            }
        } else {
            levelNum.gameObject.SetActive(false);
            levelLock.gameObject.SetActive(true);

            //level lock
            levelStatus[0].gameObject.SetActive(true);
        }
    }
}
