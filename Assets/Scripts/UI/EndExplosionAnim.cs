using UnityEngine;

public class EndExplosionAnim : MonoBehaviour {
    [SerializeField] private GameObject failMenu;



    public void EndExplosionAnimation() {
        failMenu.SetActive(true);
    }
}
