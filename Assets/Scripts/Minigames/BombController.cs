using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI timerText;
    [SerializeField]Vector2[] minigamePositions;
    [SerializeField]ModulesCombination[] possibleMinigames;
    List<MinigameBase>minigames;
    [SerializeField]BombZoomTest bombZoomTest;
    [SerializeField]RectTransform lifesLayout;
    [SerializeField]int lifes =3;
    [SerializeField]GameObject heartPrefab;
    public void Start() {
        minigames = new();
        for(int i = 0; i < lifes; i++) {
            Instantiate(heartPrefab,lifesLayout);
        }
        ModulesCombination selectedModules = possibleMinigames[Random.Range(0,possibleMinigames.Length)];
        for(int i = 0; i < selectedModules.minigames.Length; i++) {
            GameObject go = Instantiate(selectedModules.minigames[i].gameObject,transform);
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchoredPosition=minigamePositions[i];
            MinigameBase newMinigame = go.GetComponent<MinigameBase>();
            newMinigame.bombController=this;
            minigames.Add(newMinigame);
            newMinigame.Settup();
            newMinigame.zoomButton.onClick.AddListener(()=>bombZoomTest.ZoomTo(rectTransform));
        }
    }
    public void FailedMinigame() {
        LoseLife();
        bombZoomTest.TriggerShake();
    }
    public void LoseLife() {
        if (lifes>0) {
            //Destroy(lifesLayout.GetChild(lifesLayout.childCount-1).gameObject);
            Transform t = lifesLayout.GetChild(lifes-1);
            if (t != null) {
                t.GetComponent<Image>().enabled=false;
            }
        }
        lifes--;
        if (lifes <= 0) {
            //Lose Game
        }
    }
}
