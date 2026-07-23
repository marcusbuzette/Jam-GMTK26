using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
public class WireMouseControl : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField][Tooltip("Wire inteiro, ai cortado")]UILineRenderer[] uILineRendererLines;
    [SerializeField]GameObject fullWireGO;
    //[SerializeField]UILineRenderer uILineRendererCutTop;
    //[SerializeField]UILineRenderer uILineRendererCutBot;
    [SerializeField]GameObject cutWireGO;
    MiniGameWire miniGameWireController;
    bool isRightWire;
    [SerializeField]RectTransform rectTransform;
    public void OnPointerClick(PointerEventData eventData) {
        fullWireGO.SetActive(false);
        cutWireGO.SetActive(true);
        if (isRightWire) {
            miniGameWireController?.MiniGameSolved();
        } else {
            miniGameWireController?.MiniGameFailed();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        rectTransform.localScale=new Vector3(1.5f,1,1);
    }

    public void OnPointerExit(PointerEventData eventData) {
        rectTransform.localScale= Vector3.one;
    }
    public void Settup(Color c, bool isRightWire,MiniGameWire miniGameController) {
        this.isRightWire=isRightWire;
        miniGameWireController = miniGameController;
        foreach(UILineRenderer uILineRenderer in uILineRendererLines) {
            uILineRenderer.color=c;
        }
    }
}
