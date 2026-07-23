using UnityEngine;

public class DialogueManagerTest : MonoBehaviour
{
    [SerializeField] private Dialogue initialDialogue;

    private void Start()
    {
        var manager = DialogueManager.Instance;
        if (manager != null && initialDialogue != null)
        {
            manager.StartDialogue(initialDialogue);
        }
    }
}
