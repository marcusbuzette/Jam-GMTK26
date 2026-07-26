using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGameMasks : MinigameBase {
    [SerializeField]Sprite[] maskImages;
    [SerializeField]RectTransform masksLayoutGroup;
    [SerializeField]GridLayoutGroup gridLayoutGroup;
    [SerializeField]MaskButtonControl maskButtonPrefab;
    int numberOfMasks; //vamos receber esse valor de algum lugar
    List<Sprite>sprites;
    List<MaskButtonControl>masks;
    [SerializeField]MaskType correctMask;
    
    void Awake() {
        masks = new();
        sprites = new();
    }
    public override void MiniGameFailed() {
        base.MiniGameFailed();
    }

    public override void MiniGameSolved() {
        base.MiniGameSolved();
    }
    /* public void Start() {
        Settup();
    } */
    public void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            Restart();
        }
    }
    public override void Restart() {
        int childCount = masksLayoutGroup.childCount;
        for(int i = 0; i < childCount; i++) {
            GameObject go = masksLayoutGroup.transform.GetChild(0).gameObject;
            go.transform.SetParent(null);
            Destroy(go);
        }
        masks.Clear();
        sprites.Clear();
        Settup();
    }

    public override void Settup() {
        numberOfMasks = Random.Range(2,maskImages.Length+1);
        if (numberOfMasks < 4) {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 3;
        } else if(numberOfMasks < 7){
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = 2;
        }else{
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            gridLayoutGroup.constraintCount = 3;
        }
        int correctMaskInt;
        if (correctMask != MaskType.Null) {
            correctMaskInt = (int)correctMask-1;
        } else {
            correctMaskInt = Random.Range(0,numberOfMasks);
            correctMask = (MaskType)correctMaskInt+1;
        }
        for(int i = 0; i < maskImages.Length; i++) {
            if(i==correctMaskInt)continue;
            Sprite s = maskImages[i];
            sprites.Add(s);
        }
        sprites.Shuffle();
        //Adicionar a correta
        GameObject goCerto = Instantiate(maskButtonPrefab.gameObject);
        MaskButtonControl mbcC = goCerto.GetComponent<MaskButtonControl>();
        mbcC.Settup(maskImages[correctMaskInt],true,this);
        masks.Add(mbcC);
        //adicionar as erradas
        for(int i = 0; i < numberOfMasks-1; i++) {
            GameObject go = Instantiate(maskButtonPrefab.gameObject);
            MaskButtonControl mbc = go.GetComponent<MaskButtonControl>();
            mbc.Settup(sprites[i],false,this);
            masks.Add(mbc);
        }
        masks.Shuffle();
        foreach(MaskButtonControl mbc in masks) {
            RectTransform rect = mbc.GetComponent<RectTransform>();
            rect.SetParent(masksLayoutGroup);
        }
    }
    public void ClickedMask(bool isAnwser) {
        if (isAnwser) {
            MiniGameSolved();
        } else {
            MiniGameFailed();
        }
    }
}
public enum MaskType {
    Null,
    Cat,
    Dog,
    Monkey,
    Rabbit
}
public static class ListExtensions
{
    // Fisher-Yates shuffle extension method
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // UnityEngine.Random.Range is inclusive for min, exclusive for max
            int k = Random.Range(0, n + 1); 
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
