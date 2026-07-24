using UnityEngine;
using UnityEngine.UI;

public abstract class MinigameBase : MonoBehaviour
{
    public Button zoomButton;
    public abstract void Settup();
    public abstract void MiniGameSolved();//Chamado pelo fio quando cortar o fio certo, decidir aqui o que acontece
    public abstract void MiniGameFailed();//Chamado pelo fio quando cortar o fio errado, decidir aqui o que acontece
    public abstract void Restart();
}
