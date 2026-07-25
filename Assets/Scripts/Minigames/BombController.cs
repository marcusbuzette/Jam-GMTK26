using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI timerText;
    [SerializeField]Vector2[] minigamePositions;
    [SerializeField]ModulesCombination[] possibleMinigames;
    List<MinigameBase>minigames;
    [SerializeField]BombZoomTest bombZoomTest;
    public void Start() {
        minigames = new();
        ModulesCombination selectedModules = possibleMinigames[Random.Range(0,possibleMinigames.Length)];
        for(int i = 0; i < selectedModules.minigames.Length; i++) {
            GameObject go = Instantiate(selectedModules.minigames[i].gameObject,transform);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition=minigamePositions[i];
            MinigameBase newMinigame = go.GetComponent<MinigameBase>();
            minigames.Add(newMinigame);
            newMinigame.Settup();
            newMinigame.zoomButton.onClick.AddListener(()=>bombZoomTest.ZoomTo(rectTransform));
        }
    }
}
