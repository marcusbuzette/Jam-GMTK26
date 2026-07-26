using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGameWire : MinigameBase
{
    [SerializeField]GameObject wirePrefab;
    public int NumberOfWires; //vai receber isso de algum lugar

    [SerializeField]RectTransform wireLayoutGroup;
    [SerializeField]Color[] possibleColors;
    public int correctWire; //vai receber isso de algum lugar
    void Start() {
        //Settup();
    }
    public override void Settup() {
        base.Settup();
        /* NumberOfWires = Random.Range(3,5);
        correctWire = Random.Range(0,NumberOfWires);//Provisorio
        for(int i = 0; i < NumberOfWires; i++) {
            int colorIndex = Random.Range(0,possibleColors.Length);
            GameObject go=Instantiate(wirePrefab,wireLayoutGroup);
            go.GetComponent<WireMouseControl>().Settup(possibleColors[colorIndex],i==correctWire,this);
        } */
    }
    public void ReciveAnswer(List<MinigameWireAnswer>answers) {
        NumberOfWires = answers.Count;
        for(int i = 0; i < NumberOfWires; i++) {
            GameObject go=Instantiate(wirePrefab,wireLayoutGroup);
            go.GetComponent<WireMouseControl>().Settup(answers[i].color,answers[i].isAnswer,this);
        }
    }
    /* private void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            Restart();
        }
    } */
    public override void Restart() {
        int childCount = wireLayoutGroup.childCount;
        for(int i = 0; i < childCount; i++) {
            GameObject go = wireLayoutGroup.transform.GetChild(0).gameObject;
            go.transform.SetParent(null);
            Destroy(go);
        }
        Settup();
    }

    public override void MiniGameSolved() {
        base.MiniGameSolved();
    }

    public override void MiniGameFailed() {
        base.MiniGameFailed();
    }
}
