using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Light))]
public class LightAsnwer : MonoBehaviour
{
    [SerializeField]LocationsEnums spawnLocation;
    [SerializeField]MinigameType minigameType;
    List<ColorsMinigame>answer;
    Light l;
    void Awake() {
        l = GetComponent<Light>();
        MinigameAnswerController.singleton.InformMinigameSpawnLocation(minigameType,spawnLocation);
        answer=MinigameAnswerController.singleton.GetColorsAwnser();
    }
    void Start() {
        StartCoroutine(nameof(BlinkLights));
        
    }
    int index=0;
    IEnumerator BlinkLights() {
        while(true){
            l.color=GetColor(index);
            yield return new WaitForSeconds(1f);
            index++;
            if (index >= answer.Count) {
                index=index%answer.Count;
                l.color = Color.black;
                yield return new WaitForSeconds(2f);  
            }
        }
    }
    Color GetColor(int index) {
        if(index>answer.Count){
            Debug.LogWarning("Erro no getColor");
            return Color.black;
        }
        switch (answer[index]) {
            case ColorsMinigame.red:
            return Color.red;
            case ColorsMinigame.blue:
            return Color.blue;
            case ColorsMinigame.green:
            return Color.green;
            case ColorsMinigame.yellow:
            return Color.yellow;
            default: return Color.black;
        }
    }
}
