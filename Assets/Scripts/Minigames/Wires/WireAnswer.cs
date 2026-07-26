using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WireAnswer : MonoBehaviour
{
    [SerializeField]LocationsEnums spawnLocation;
    [SerializeField]MinigameType minigameType;
    [SerializeField]InteractableItem interactableItem;
    List<MinigameWireAnswer> answers;
    string clue;
    void Awake() {
        MinigameAnswerController.singleton.InformMinigameSpawnLocation(minigameType,spawnLocation);
        answers=MinigameAnswerController.singleton.GetWireAnswers(out clue);
        GameObject go = interactableItem.GetContentPrefab();
        go.GetComponentInChildren<TextMeshProUGUI>().text=clue;
    }

}
