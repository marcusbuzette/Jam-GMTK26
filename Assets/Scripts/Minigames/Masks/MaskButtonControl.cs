using UnityEngine;
using UnityEngine.UI;

public class MaskButtonControl : MonoBehaviour
{
    [SerializeField]Image image;
    [SerializeField]Button button;
    [SerializeField]bool isAnwser;
    MiniGameMasks miniGameController;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip buttonPress;
    
    public void Settup(Sprite s,bool isAnwser,MiniGameMasks miniGameMasks) {
        image.sprite=s;
        this.isAnwser = isAnwser;
        miniGameController=miniGameMasks;
    }
    public void Clicked() {
        miniGameController?.ClickedMask(isAnwser);
        audioPlayerLocal?.PlayAudioClip(buttonPress);
        
    }
    
}
