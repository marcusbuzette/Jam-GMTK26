using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-1)]
public class MinigameAnswerController : MonoBehaviour
{
    public static MinigameAnswerController singleton;
    Dictionary<MinigameType,LocationsEnums> miniGameSpawnLocations;
    public void Awake() {
        if (singleton == null) {
            singleton=this;
            miniGameSpawnLocations=new();
        }
    }
    public void ProvideAnswer(MinigameBase minigameBase) {
        switch (minigameBase) {
            case MiniGameWire miniGameWire:
                Debug.Log("Um wire pediu uma resposta");

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
}
public enum MinigameType {
    Wire,
    CountDown,
    Masks,
    CodeWords,
    Colors,
    Numbers
}
