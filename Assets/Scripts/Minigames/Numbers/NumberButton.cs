using UnityEngine;

public class NumberButton : MonoBehaviour {
    [SerializeField]int numberEffected;
    [SerializeField]bool isUp;
    [SerializeField]MiniGameNumbers miniGameNumbers;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip decrease;
    public void Clickled() {
        miniGameNumbers?.ButtonClicked(numberEffected,isUp);
        audioPlayerLocal?.PlayAudioClip(decrease);
        
    }
}
