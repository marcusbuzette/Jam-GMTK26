using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractPannelController : MonoBehaviour {
    public static InteractPannelController Instance;

    [SerializeField] private RectTransform contentContainer;
    [SerializeField] private GameObject panelWrapper;

    [SerializeField] private Button closeButton;

    public bool IsInteractPanelOpen {
        get {
            return panelWrapper.activeSelf;
        }
    }

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ShowPannel(GameObject content) {
        if (contentContainer.childCount > 0) {
            for (int i = contentContainer.childCount - 1; i >= 0; i--) {
                Destroy(contentContainer.GetChild(i));
            }
        }
        LevelManager.Instance.HandleOpenedPannel();
        closeButton.onClick.AddListener(() => HidePannel());
        Instantiate(content, contentContainer);
        panelWrapper.SetActive(true);
    }

    public void HidePannel() {
        LevelManager.Instance.HandleClosedPannel();
        closeButton.onClick.RemoveAllListeners();
        panelWrapper.SetActive(false);
    }
}
