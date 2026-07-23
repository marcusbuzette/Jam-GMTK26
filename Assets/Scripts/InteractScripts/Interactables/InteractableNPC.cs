using UnityEngine;

public class InteractableNPC : InteractableBase {
    public override void Interact(GameObject interactor)
    {
        Debug.Log("Iniciando conversa com o NPC...");
        // Aqui você chamaria seu DialogueManager.StartDialogue(...)
    }

}
