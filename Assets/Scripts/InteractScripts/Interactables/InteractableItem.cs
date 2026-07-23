using UnityEngine;

public class InteractableItem : InteractableBase {
    public override void Interact(GameObject interactor)
    {
        Debug.Log("Interagindo com item");
    }
}
