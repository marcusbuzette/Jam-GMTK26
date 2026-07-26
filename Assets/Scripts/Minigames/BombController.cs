using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BombController : MonoBehaviour
{
    public static BombController singleton;
    [SerializeField]TextMeshProUGUI timerText;
    [SerializeField]Vector2[] minigamePositions;
    [SerializeField]ModulesCombination[] possibleMinigames;
    List<MinigameBase>minigames;
    [SerializeField]BombZoomTest bombZoomTest;
    [SerializeField]RectTransform lifesLayout;
    [SerializeField]int lifes =3;
    [SerializeField]GameObject heartPrefab;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip ModuleFailure;
    [SerializeField]AudioClip ModuleCompleted;
    int nMinigames;
    int completedMinigames;
    public bool isOpen = false;
    public void Awake() {
        if (singleton == null) {
            singleton=this;
        }
    }
    public void Start() {
        minigames = new();
        for(int i = 0; i < lifes; i++) {
            Instantiate(heartPrefab,lifesLayout);
        }
        ModulesCombination selectedModules = possibleMinigames[Random.Range(0,possibleMinigames.Length)];
        nMinigames = selectedModules.minigames.Length;
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
        audioPlayerLocal?.PlayAudioClip(ModuleFailure);
    }
    public void MinigameSuccess() {
        audioPlayerLocal?.PlayAudioClip(ModuleCompleted);
        completedMinigames++;
        if (completedMinigames >= nMinigames) {
            LevelManager.Instance.TriggerVictory();
        }
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
            LevelManager.Instance.TriggerDefeat("Erros d+");
        }
    }
    void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            OpenCloseBomb();
        }
    }
    public void OpenCloseBomb() {
        bool attempt =false;
        if (isOpen) {//se ta aberto ent fecha
            attempt=bombZoomTest.OpenClose(false);
            if(attempt)LevelManager.Instance.HandleClosedPannel();
        } else {//se ta fechado ent abre
            attempt=bombZoomTest.OpenClose(true); 
        }
        if(attempt)isOpen=!isOpen;
    }
    public void UpdateTimer(float RemainingTime) {
        int minutes = Mathf.FloorToInt(RemainingTime / 60f);
        
        // Get the remainder after dividing by 60 to get seconds
        int seconds = Mathf.FloorToInt(RemainingTime % 60f);

        // "D2" forces the string to display at least 2 digits (adds leading zero)
        timerText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }
}
