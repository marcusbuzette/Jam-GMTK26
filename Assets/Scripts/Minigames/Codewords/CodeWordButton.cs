using TMPro;
using UnityEngine;

public class CodeWordButton : MonoBehaviour
{
    MiniGameCodeWords miniGameCodeWords;
    bool isAnwser;
    string word;
    [SerializeField]TextMeshProUGUI text;
    [SerializeField]AudioPlayerLocal audioPlayerLocal;
    [SerializeField]AudioClip buttonPress;
    public void Settup(bool isAnwser,string word,MiniGameCodeWords miniGameController) {
        this.isAnwser=isAnwser;
        this.word=word;
        text.text=word;
        miniGameCodeWords=miniGameController;
    }
    public void Clicked() {
        miniGameCodeWords?.ClickedOption(isAnwser);
        audioPlayerLocal?.PlayAudioClip(buttonPress);
    }
    
}
