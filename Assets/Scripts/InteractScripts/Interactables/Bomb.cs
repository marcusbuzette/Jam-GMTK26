using UnityEngine;

public class Bomb : InteractableBase {
    public override void Interact(GameObject interactor) {
        BombController.singleton.OpenCloseBomb();
        LevelManager.Instance.HandleOpenedPannel();
        //LevelManager.Instance.TriggerVictory();
    }

}
