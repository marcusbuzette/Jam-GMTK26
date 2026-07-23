using UnityEngine;

public class Bomb : InteractableBase {
    public override void Interact(GameObject interactor) {
        Debug.Log("Interagindo com bomba");
        LevelManager.Instance.TriggerVictory();
    }

}
