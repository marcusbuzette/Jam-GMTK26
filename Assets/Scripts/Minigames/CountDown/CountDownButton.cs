using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountDownButton : MonoBehaviour, IPointerUpHandler,IPointerDownHandler
{
    [SerializeField]Button button;
    [SerializeField]MiniGameCountDown miniGameCountDown;
    
    public void OnPointerDown(PointerEventData eventData) {
       miniGameCountDown.PressedButton();
    }

    public void OnPointerUp(PointerEventData eventData) {
        miniGameCountDown.ReleasedButton();
    }

}
