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
        Settup();
    }
    public override void Settup() {
        correctWire = Random.Range(0,NumberOfWires);
        for(int i = 0; i < NumberOfWires; i++) {
            int colorIndex = Random.Range(0,possibleColors.Length);
            GameObject go=Instantiate(wirePrefab,wireLayoutGroup);
            go.GetComponent<WireMouseControl>().Settup(possibleColors[colorIndex],i==correctWire,this);
        }
    }
    private void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            Restart();
        }
    }
    public override void Restart() {
        int childCount = wireLayoutGroup.childCount;
        for(int i = 0; i < childCount; i++) {
            GameObject go = wireLayoutGroup.transform.GetChild(0).gameObject;
            go.transform.parent=null;
            Destroy(go);
        }
        Settup();
    }

    public override void MiniGameSolved() {
        Debug.Log("O detetive mais brabo da historia");
    }

    public override void MiniGameFailed() {
        Debug.Log("Kabum!");
    }
}
