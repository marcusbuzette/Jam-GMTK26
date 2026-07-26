using TMPro;
using UnityEngine;

public class MiniGameNumbers : MinigameBase {
    [SerializeField]int[]numbers;
    [SerializeField]TextMeshProUGUI[] numbersText;
    [SerializeField]int[]answer;
    public override void Restart() {
        
    }

    public override void Settup() {
        for(int i = 0; i < numbers.Length; i++) {
            numbers[i]=Random.Range(0,10);
            numbersText[i].text=numbers[i].ToString();
        }
        for(int i = 0; i < answer.Length; i++) {
            answer[i]=Random.Range(0,10);
        }
    }
    public void ButtonClicked(int numberEffected,bool isUp) {
        int mod = isUp?1:-1;
        numbers[numberEffected-1]=numbers[numberEffected-1]+mod;
        if(numbers[numberEffected-1]>9)numbers[numberEffected-1]=numbers[numberEffected-1]%10;
        if(numbers[numberEffected-1]<0)numbers[numberEffected-1]=9;
        numbersText[numberEffected-1].text=numbers[numberEffected-1].ToString();
    }
    public void TryCode() {
        bool isCorrect=true;
        for(int i = 0; i < numbers.Length; i++) {
            if (numbers[i] != answer[i]) {
                isCorrect=false;
                break;
            }
        }
        if (isCorrect) {
            MiniGameSolved();
        } else {
            MiniGameFailed();
        }
    }
}
