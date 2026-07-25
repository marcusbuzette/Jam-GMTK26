using UnityEngine;

public class NumberButton : MonoBehaviour {
    [SerializeField]int numberEffected;
    [SerializeField]bool isUp;
    [SerializeField]MiniGameNumbers miniGameNumbers;
    public void Clickled() {
        miniGameNumbers?.ButtonClicked(numberEffected,isUp);
    }
}
