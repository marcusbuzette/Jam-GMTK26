using System.Collections.Generic;
using UnityEngine;

public class MiniGameColors : MinigameBase {
    List<ColorsMinigame> answer;
    public List<int> answerDebug;
    List<ColorsMinigame> attempt;
    public List<int> attemptDebug;
    public int numberOfSteps;
    bool alreadyCompleted;
    public override void MiniGameFailed() {
        base.MiniGameFailed();
        Restart();
    }
    public void Awake() {
        answer = new();
        answerDebug=new();
        attempt = new();
        attemptDebug = new();
    }
    /* void Start() {
        Settup();
    } */
    public override void MiniGameSolved() {
        base.MiniGameSolved();
    }

    public override void Restart() {
        answer.Clear();
        attempt.Clear();
        attemptDebug.Clear();
        answerDebug.Clear();
        Settup();
    }

    public override void Settup() {
        numberOfSteps = Random.Range(4,6);
        for(int i = 0; i < numberOfSteps; i++) {
            int aux = Random.Range(0,4);
            answer.Add((ColorsMinigame)aux);
            answerDebug.Add(aux);
        }
    }
    public void ClickedColor(ColorsMinigame color) {
        if(!alreadyCompleted)
            AttempSolve(color);
    }
    void AttempSolve(ColorsMinigame color) {
        attempt.Add(color);
        attemptDebug.Add((int)color);
        TestCorrectness();
    }
    void TestCorrectness() {
        if(attempt==null||answer==null)return;
        if(attempt.Count>answer.Count)return;
        /* if(attempt.Count != answer.Count){
            Debug.LogWarning("List de attempt e respostas com sizes diferentes");
            return;
        } */
        bool correct = false;
        for(int i = 0; i < attempt.Count;i++) {
            if (attempt[i] == answer[i]) {
                correct=true;
            } else {
                correct=false;
                break;
            }
        }
        if (correct) {
            if(attempt.Count==numberOfSteps){
                alreadyCompleted=true;
                MiniGameSolved();
            }
        } else {
            MiniGameFailed();
        }
    }
}
public enum ColorsMinigame {
    red,
    blue,
    green,
    yellow
}
