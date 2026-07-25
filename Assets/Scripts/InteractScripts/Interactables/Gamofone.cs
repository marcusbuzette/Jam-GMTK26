using UnityEngine;

public class Gamofone : InteractableBase {
    public override void Interact(GameObject interactor) {
        LevelManager.Instance.TurnOnNowPlayingUI();
    }
}
