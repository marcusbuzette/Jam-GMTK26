using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-1)]
public class MinigameAnswerController : MonoBehaviour
{
    public static MinigameAnswerController singleton;
    Dictionary<MinigameType,LocationsEnums> miniGameSpawnLocations;
    List<MinigameBase>minigames;
    public void Awake() {
        if (singleton == null) {
            singleton=this;
            miniGameSpawnLocations=new();
            minigames=new();
        }
    }
    public void AddMinigame(MinigameBase minigameBase) {
        minigames.Add(minigameBase);
        ProvideAnswer(minigameBase.minigameType);
    }
    public void ProvideAnswer(MinigameType minigameType) {
        MinigameBase m = minigames.Find((o)=>o.minigameType==minigameType);
        if(m==null){
            Debug.Log($"Couldn't find a minigame of said type {minigameType} in list ");
            Debug.Log($"minigame types in list, of size {minigames.Count}");
            for(int i = 0; i < minigames.Count; i++) {
                Debug.Log($"{minigames[i].minigameType}, ");
            }
        }
        switch (m) {
            case MiniGameWire miniGameWire:
                Debug.Log("Um wire pediu uma resposta");
                string s;
                miniGameWire.ReciveAnswer(GetWireAnswers(out s));
            break;
            case MiniGameCountDown miniGameCountDown:
                Debug.Log("Um countDown pediu uma resposta");
            break;
            case MiniGameMasks miniGameMasks:
                Debug.Log("Um masks pediu uma resposta");
            break;
            case MiniGameCodeWords miniGameCodeWords:
                Debug.Log("Um codeWords pediu uma resposta");
            break;
            case MiniGameColors miniGameColors:
                Debug.Log("Um colors pediu uma resposta");
                miniGameColors.ReciveAnswer(GetColorsAwnser());
            break;
            case MiniGameNumbers:
                Debug.Log("Um numbers pediu uma resposta");
            break;
            case MinigameBase minigameBase2:
                Debug.Log("Um base pediu uma resposta");
            break;
        }
    }
    public void InformMinigameSpawnLocation(MinigameType minigameType, LocationsEnums spawnLocation) {
        miniGameSpawnLocations.Add(minigameType,spawnLocation);
        //ProvideAnswer(minigameType);
    }
    public List<ColorsMinigame> GetColorsAwnser() {
        List<ColorsMinigame> answer = new();
        if(miniGameSpawnLocations.TryGetValue(MinigameType.Colors,out LocationsEnums location)){
            switch (location) {
                case LocationsEnums.BallRoomF1:
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.blue);
                break;
                case LocationsEnums.BallRoomF2:
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.yellow);
                break;
                case LocationsEnums.Bedroom:
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.green);
                break;
                case LocationsEnums.Kitchen:
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.green);
                break;
                case LocationsEnums.Library:
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.red);
                    answer.Add(ColorsMinigame.green);
                break;
                case LocationsEnums.Office:
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.red);
                break;
                case LocationsEnums.Storage:
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.yellow);
                    answer.Add(ColorsMinigame.blue);
                    answer.Add(ColorsMinigame.green);
                    answer.Add(ColorsMinigame.yellow);
                break;
            }
        } else {
            answer.Add(ColorsMinigame.green);
            answer.Add(ColorsMinigame.blue);
            answer.Add(ColorsMinigame.red);
            answer.Add(ColorsMinigame.red);
            answer.Add(ColorsMinigame.yellow);
        }
        return answer;
    }
    public List<MinigameWireAnswer> GetWireAnswers(out string clue) {
        List<MinigameWireAnswer>answer=new();
        clue = "I dunno dog, good luck";
        if(miniGameSpawnLocations.TryGetValue(MinigameType.Wire,out LocationsEnums location)){
            switch (location) {
                case LocationsEnums.BallRoomF1://Azul
                    answer.Add(new MinigameWireAnswer(false,Color.red));
                    answer.Add(new MinigameWireAnswer(false,Color.green));
                    answer.Add(new MinigameWireAnswer(true,Color.blue));
                    answer.Add(new MinigameWireAnswer(false,Color.white));
                    clue = "Cut the blue wire";
                break;
                case LocationsEnums.BallRoomF2://Azul
                    answer.Add(new MinigameWireAnswer(false,Color.green));
                    answer.Add(new MinigameWireAnswer(true,Color.blue));
                    answer.Add(new MinigameWireAnswer(false,Color.black));
                    answer.Add(new MinigameWireAnswer(false,Color.white));
                    clue = "Cut the blue wire";
                break;
                case LocationsEnums.Bedroom: //blue //clue wrong
                    answer.Add(new MinigameWireAnswer(false,Color.blue));
                    answer.Add(new MinigameWireAnswer(true,Color.yellow));
                    answer.Add(new MinigameWireAnswer(false,Color.green));
                    answer.Add(new MinigameWireAnswer(true,Color.blue));
                    clue = "Cut green wire";
                break;
                case LocationsEnums.Kitchen://Yellow //clue wrong
                    answer.Add(new MinigameWireAnswer(false,Color.green));
                    answer.Add(new MinigameWireAnswer(true,Color.yellow));
                    answer.Add(new MinigameWireAnswer(false,Color.blue));
                    answer.Add(new MinigameWireAnswer(false,Color.black));
                    clue = "Cut green wire";
                break;
                case LocationsEnums.Library://white
                    answer.Add(new MinigameWireAnswer(false,Color.red));
                    answer.Add(new MinigameWireAnswer(false,Color.blue));
                    answer.Add(new MinigameWireAnswer(true,Color.white));
                    clue = "Cut the fourth wire :)";
                break;
                case LocationsEnums.Office://White
                    answer.Add(new MinigameWireAnswer(false,Color.yellow));
                    answer.Add(new MinigameWireAnswer(false,Color.red));
                    answer.Add(new MinigameWireAnswer(true,Color.white));
                    clue = "Cut the fourth wire :)";
                break;
                case LocationsEnums.Storage://Green 
                    answer.Add(new MinigameWireAnswer(false,Color.red));
                    answer.Add(new MinigameWireAnswer(false,Color.red));
                    answer.Add(new MinigameWireAnswer(false,Color.green));
                    answer.Add(new MinigameWireAnswer(true,Color.green));
                    clue = "Cut the green wire :)";
                break;
            }
        } else {
            answer.Add(new MinigameWireAnswer(true,Color.red));
            answer.Add(new MinigameWireAnswer(false,Color.red));
            answer.Add(new MinigameWireAnswer(false,Color.green));
            answer.Add(new MinigameWireAnswer(false,Color.blue));
            answer.Add(new MinigameWireAnswer(false,Color.white));
            clue = "Didn't find dictionary";
        }
        return answer;
    }
}
public enum MinigameType {
    Wire,
    CountDown,
    Masks,
    CodeWords,
    Colors,
    Numbers
}
public struct MinigameWireAnswer{
    public bool isAnswer;
    public Color color;
    public MinigameWireAnswer(bool isAnswer, Color c) {
        this.isAnswer=isAnswer;
        color=c;
    }
}
