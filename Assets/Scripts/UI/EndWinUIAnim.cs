using UnityEngine;

public class EndWinUIAnim : MonoBehaviour {
    [SerializeField] private GameObject winMenu;



    public void EndExplosionAnimation() {
        winMenu.SetActive(true);
    }
}
