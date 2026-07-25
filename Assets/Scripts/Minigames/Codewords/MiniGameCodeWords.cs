using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGameCodeWords : MinigameBase {
    [SerializeField]CodeWordButton codeWordButtonPrefab;
    [SerializeField]GridLayoutGroup gridLayoutGroup;
    public int numberOfOptions;
    public CodeWordOptions codeWordOptions;
    public int answerIndex;
    List<string> codeWorldOptionsList;
    List<CodeWordButton> codeWordButtonsList;
    [SerializeField]bool isEnglish=true;
    public string rightOptionDebug;
    void Awake() {
        codeWorldOptionsList = new();
        codeWordButtonsList = new();
    }
    /* void Start() {
        Settup();
    } */
    public override void MiniGameFailed() {
        base.MiniGameFailed();
    }

    public override void MiniGameSolved() {
        base.MiniGameSolved();
    }
    void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            Restart();
        }
    }
    public override void Restart() {
        codeWordButtonsList.Clear();
        codeWorldOptionsList.Clear();
        int childCount = gridLayoutGroup.transform.childCount;
        for(int i = 0; i < childCount; i++) {
            GameObject go = gridLayoutGroup.transform.GetChild(0).gameObject;
            go.transform.SetParent(null);
            Destroy(go);
        }
        Settup();
    }

    public override void Settup() {
        //Get language, assume english for now
        string[] options;
        if(isEnglish){
            options= codeWordOptions.codeWordsOptionsEnglish;
        }
        else {
            options = codeWordOptions.codeWordsOptionsPortugues;
        }
        numberOfOptions = UnityEngine.Random.Range(4,options.Length+1);
        answerIndex = UnityEngine.Random.Range(0,numberOfOptions);
        Vector2 spacing = gridLayoutGroup.spacing;
        spacing.y = gridLayoutGroup.GetComponent<RectTransform>().rect.height/numberOfOptions;
        gridLayoutGroup.spacing = spacing;
        for(int i = 0; i < options.Length; i++) {
            if(i!=answerIndex)codeWorldOptionsList.Add(options[i]);
        }
        codeWorldOptionsList.Shuffle();
        GameObject go=Instantiate(codeWordButtonPrefab.gameObject);
        CodeWordButton auxButton = go.GetComponent<CodeWordButton>();
        auxButton.Settup(true,options[answerIndex],this);
        rightOptionDebug=options[answerIndex];
        codeWordButtonsList.Add(auxButton);
        for(int i = 0; i < numberOfOptions-1; i++) {
            go=Instantiate(codeWordButtonPrefab.gameObject);
            auxButton = go.GetComponent<CodeWordButton>();
            auxButton.Settup(false,codeWorldOptionsList[i],this);
            codeWordButtonsList.Add(auxButton);
        }
        codeWordButtonsList.Shuffle();
        for(int i = 0; i < codeWordButtonsList.Count; i++) {
            codeWordButtonsList[i].GetComponent<RectTransform>().SetParent(gridLayoutGroup.transform);
        }
    }
    public void ClickedOption(bool isAnwser) {
        if (isAnwser) {
            MiniGameSolved();
        } else {
            MiniGameFailed();
        }
    }
    

}

