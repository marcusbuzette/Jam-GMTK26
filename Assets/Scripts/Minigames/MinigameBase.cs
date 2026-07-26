using UnityEngine;
using UnityEngine.UI;

public abstract class MinigameBase : MonoBehaviour
{
    public Image state;
    public Sprite chechMark;
    public Sprite redX;
    public Button zoomButton;
    public BombController bombController;
    bool isZoomed =false;
    public Image cover;
    public abstract void Settup();
    public virtual void MiniGameSolved(){
        state.sprite=chechMark;
        cover.enabled=true;
        bombController.MinigameSuccess();
        Debug.Log("O detetive mais brabo da historia");
    }
    public virtual void MiniGameFailed() {
        bombController.FailedMinigame();
        Debug.Log("Kabum!");
    }
    public abstract void Restart();

    public void ToggleZoom() {//Chamado pelo zoom da bomba
        if (isZoomed) {
            isZoomed=false;
            zoomButton.GetComponent<Image>().enabled=true;
        } else {
            isZoomed=true;
            zoomButton.GetComponent<Image>().enabled=false;
        }
    }
}
