using UnityEngine;

public class BlinkingIndicator : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField, Min(0.01f)] private float blinkSpeed = 1f;
    [SerializeField] private bool startVisible = true;

    private CanvasGroup canvasGroup;
    private float time;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = startVisible ? 1f : 0f;
    }

    private void Update()
    {
        time += Time.deltaTime * blinkSpeed;
        canvasGroup.alpha = Mathf.PingPong(time, 1f);
    }
}
