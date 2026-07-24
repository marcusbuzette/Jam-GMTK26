using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGameCountDown : MinigameBase {
    bool timerIsRunning;
    [SerializeField]TextMeshProUGUI placeText;
    [SerializeField]TextMeshProUGUI timerText;
    [SerializeField] float elapsedTime;
    [SerializeField]string[] places;
    [SerializeField] float correctTime;
    [SerializeField]float maxTimeDiference;
    public override void MiniGameFailed() {
        Debug.Log("Kabum");
    }

    public override void MiniGameSolved() {
        Debug.Log("O detetive mais brabo da historia");
    }
    public override void Restart() {
        timerIsRunning=false;
        timerText.color=Color.black;
        timerText.text="00:00";
        placeText.text="";
    }

    public override void Settup() {
        timerIsRunning=false;
        timerText.color=Color.black;
        timerText.text="00:00";
        placeText.text="";
    }
    public void ReleasedButton() {
        float timeDiference = correctTime - elapsedTime;
        timeDiference = Mathf.Abs(timeDiference);
        timerIsRunning=false;
        if (timeDiference < maxTimeDiference) {
            timerText.color=Color.green;
            MiniGameSolved();
        } else {
            MiniGameFailed();
        }
    }
    public void PressedButton() {
        //Start timer
        elapsedTime = Random.Range(10,15);
        timerText.color=Color.red;
        timerIsRunning=true;
        //Show random name place
        placeText.text=places[Random.Range(0,places.Length)];
        //Select correct option to be right
        correctTime = Random.Range(1.0f,9.9f);
    }
    public void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            Restart();
        }
        if (Keyboard.current.sKey.wasPressedThisFrame) {
            Time.timeScale=.5f;
        }
        if (Keyboard.current.wKey.wasPressedThisFrame) {
            Time.timeScale=1;
        }
        if (timerIsRunning) {
            elapsedTime -= Time.deltaTime;

            // Extract whole seconds
            int seconds = Mathf.FloorToInt(elapsedTime);
        
            // Extract the decimal remainder and scale to milliseconds
            int milliseconds = Mathf.FloorToInt((elapsedTime % 1f) * 100f);

            // Format to a string using string interpolation
            timerText.text = $"{seconds:00}:{milliseconds:00}";
        }
    }
}
//Timer 00:00 descendo, ai precisa soltar o botão quando ficar proximo do numero, que vai depender do comodo de onde a bomba está
