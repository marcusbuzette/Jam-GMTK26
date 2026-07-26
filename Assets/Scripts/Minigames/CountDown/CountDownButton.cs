using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountDownButton : MonoBehaviour, IPointerUpHandler,IPointerDownHandler
{
    [SerializeField]Button button;
    [SerializeField]MiniGameCountDown miniGameCountDown;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip buttonPRess;
    [SerializeField]AudioClip buttonRelease;
    
    public void OnPointerDown(PointerEventData eventData) {
       miniGameCountDown.PressedButton();
       audioPlayerLocal?.PlayAudioClip(buttonPRess);
    }

    public void OnPointerUp(PointerEventData eventData) {
        miniGameCountDown.ReleasedButton();
        audioPlayerLocal?.PlayAudioClip(buttonRelease);
    }

}
