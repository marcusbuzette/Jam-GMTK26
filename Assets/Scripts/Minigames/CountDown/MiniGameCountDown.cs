using UnityEngine;

public class MiniGameCountDown : MinigameBase {
    bool timerIsRunning;
    public override void MiniGameFailed() {
        throw new System.NotImplementedException();
    }

    public override void MiniGameSolved() {
        throw new System.NotImplementedException();
    }

    public override void Restart() {
        throw new System.NotImplementedException();
    }

    public override void Settup() {
        throw new System.NotImplementedException();
    }
    public void ReleasedButton() {
        //If timer is close enough win
    }
    public void PressedButton() {
        //Start timer
    }
    public void Update() {
        if (timerIsRunning) {
            //Countdown time
        }
    }
    //Timer 00:00 descendo, ai precisa soltar o botão quando ficar proximo do numero, que vai depender do comodo de onde a bomba está
}
