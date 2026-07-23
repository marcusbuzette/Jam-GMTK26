using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTextAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Typing")]
    [SerializeField, Min(0.01f)] private float charactersPerSecond = 20f;

    private Coroutine typingRoutine;
    private string fullText = string.Empty;
    private bool isFullyDisplayed;

    public event System.Action TypingFinished;

    public TextMeshProUGUI TextComponent => textComponent;

    public bool IsTyping => typingRoutine != null;
    public bool IsFullyDisplayed => isFullyDisplayed;

    public void ShowText(string text)
    {
        if (textComponent == null)
        {
            return;
        }

        fullText = text ?? string.Empty;
        isFullyDisplayed = false;
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        typingRoutine = StartCoroutine(TypeTextRoutine(fullText));
    }

    public void SkipTyping()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (textComponent != null)
        {
            RevealAllCharacters(fullText);
        }

        isFullyDisplayed = true;
        TypingFinished?.Invoke();
    }

    public void Clear()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (textComponent != null)
        {
            textComponent.text = string.Empty;
            textComponent.maxVisibleCharacters = 0;
        }

        fullText = string.Empty;
        isFullyDisplayed = false;
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        if (textComponent == null)
        {
            yield break;
        }

        textComponent.text = text;
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();

        var totalCharacters = textComponent.textInfo.characterCount;
        var delay = 1f / charactersPerSecond;

        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }

        typingRoutine = null;
        isFullyDisplayed = true;
        TypingFinished?.Invoke();
    }

    private void RevealAllCharacters(string text)
    {
        textComponent.text = text ?? string.Empty;
        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
    }
}
