using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractPannelController : MonoBehaviour {
    public static InteractPannelController Instance;

    [SerializeField] private RectTransform contentContainer;

    [SerializeField] private Button closeButton;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void OnEnable() {
        LevelManager.Instance.HandleOpenedPannel();
        closeButton.onClick.AddListener(() => HidePannel());
    }

    public void ShowPannel(GameObject content) {
        if (contentContainer.childCount > 0) {
            for (int i = contentContainer.childCount - 1; i >= 0; i--) {
                Destroy(contentContainer.GetChild(i));
            }
        }

        Instantiate(content, contentContainer);
    }

    public void HidePannel() {
        closeButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    private void OnDisable() {
        LevelManager.Instance.HandleClosedPannel();
    }
}
