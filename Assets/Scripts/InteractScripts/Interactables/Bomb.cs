using UnityEngine;

public class Bomb : InteractableBase {
    public override void Interact(GameObject interactor) {
        LevelManager.Instance.TriggerVictory();
    }

}
