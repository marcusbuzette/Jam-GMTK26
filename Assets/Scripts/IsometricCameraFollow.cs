using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour
{
    [Header("Dialogue Zoom")]
    [SerializeField] private float dialogueOrthographicSize = 5f;
    [SerializeField, Min(0.01f)] private float zoomSpeed = 4f;

    private Camera targetCamera;
    private DialogueManager subscribedManager;
    private float defaultOrthographicSize;
    private float targetOrthographicSize;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            defaultOrthographicSize = targetCamera.orthographicSize;
            targetOrthographicSize = defaultOrthographicSize;
        }

        SubscribeToDialogueManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromDialogueManager();
    }

    private void Update()
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        targetCamera.orthographicSize = Mathf.MoveTowards(
            targetCamera.orthographicSize,
            targetOrthographicSize,
            zoomSpeed * Time.deltaTime);
    }

    private void OnDialogueStarted(Dialogue dialogue)
    {
        targetOrthographicSize = dialogueOrthographicSize;
    }

    private void OnDialogueFinished(Dialogue dialogue)
    {
        targetOrthographicSize = defaultOrthographicSize;
    }

    private void SubscribeToDialogueManager()
    {
        var manager = DialogueManager.Instance;
        if (manager == null || manager == subscribedManager)
        {
            return;
        }

        UnsubscribeFromDialogueManager();
        subscribedManager = manager;
        subscribedManager.DialogueStarted += OnDialogueStarted;
        subscribedManager.DialogueFinished += OnDialogueFinished;
    }

    private void UnsubscribeFromDialogueManager()
    {
        if (subscribedManager == null)
        {
            return;
        }

        subscribedManager.DialogueStarted -= OnDialogueStarted;
        subscribedManager.DialogueFinished -= OnDialogueFinished;
        subscribedManager = null;
    }
}
