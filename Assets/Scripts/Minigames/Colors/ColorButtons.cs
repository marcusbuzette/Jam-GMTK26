using System.Collections;
using UnityEngine;

public class ColorButtons : MonoBehaviour
{   
    [SerializeField]MiniGameColors miniGameController;
    [SerializeField]ColorsMinigame colorsMinigame;
    public Vector2 moveAmountPressed = new Vector2(0,-10);
    public float duration = 0.3f;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip buttonPress;
    public void Clicked() {
        miniGameController?.ClickedColor(colorsMinigame);
        audioPlayerLocal?.PlayAudioClip(buttonPress);
        StartCoroutine(nameof(PressAnimation));
    }
    IEnumerator PressAnimation() {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 startingPos = rectTransform.anchoredPosition;
        float timer=0;
        while(timer<duration){
            timer += Time.deltaTime;
            float progress = timer/duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startingPos,startingPos-moveAmountPressed,progress);
            yield return new WaitForEndOfFrame();
        }
        timer=0;
        while(timer<duration){
            timer += Time.deltaTime;
            float progress = timer/duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startingPos-moveAmountPressed,startingPos,progress);
            yield return new WaitForEndOfFrame();
        }
    }
}
