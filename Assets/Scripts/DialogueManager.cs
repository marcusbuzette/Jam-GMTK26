using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    public static DialogueManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<DialogueManager>();
            }

            return instance;
        }
    }

    [Header("Current Dialogue")]
    [SerializeField] private Dialogue currentDialogue;
    [SerializeField] private string currentLanguageCode;

    private int currentLineIndex;
    private bool awaitingChoiceSelection;

    public event Action<Dialogue> DialogueStarted;
    public event Action<Dialogue> DialogueAdvanced;
    public event Action<Dialogue> DialogueFinished;

    public Dialogue CurrentDialogue => currentDialogue;
    public int CurrentLineIndex => currentLineIndex;
    public string CurrentLanguageCode => currentLanguageCode;

    public bool HasDialogue => currentDialogue != null;

    public bool HasChoices => currentDialogue != null && currentDialogue.DialogueChoice != null && currentDialogue.DialogueChoice.HasOptions;

    public bool IsAwaitingChoiceSelection => awaitingChoiceSelection;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void StartDialogue(Dialogue dialogue, string languageCode = null)
    {
        if (dialogue == null)
        {
            return;
        }

        currentDialogue = dialogue;
        currentLanguageCode = string.IsNullOrWhiteSpace(languageCode)
            ? Dialogue.GetCurrentLanguageCode()
            : languageCode;
        currentLineIndex = 0;
        awaitingChoiceSelection = false;

        DialogueStarted?.Invoke(currentDialogue);

        if (currentDialogue.Lines.Count == 0 && HasChoices)
        {
            awaitingChoiceSelection = true;
        }

        DialogueAdvanced?.Invoke(currentDialogue);
    }

    public void SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return;
        }

        currentLanguageCode = languageCode;
    }

    public DialogueLine GetCurrentLine()
    {
        if (currentDialogue == null)
        {
            return null;
        }

        return currentDialogue.GetLine(currentLineIndex);
    }

    public string GetCurrentLineText()
    {
        var line = GetCurrentLine();
        return line != null ? line.GetTextForLanguage(currentLanguageCode) : string.Empty;
    }

    public string GetCurrentSpeakerName()
    {
        var line = GetCurrentLine();
        return line != null ? line.GetSpeakerName() : string.Empty;
    }

    public void AdvanceLine()
    {
        if (currentDialogue == null)
        {
            return;
        }

        if (currentLineIndex < currentDialogue.Lines.Count - 1)
        {
            currentLineIndex++;
            DialogueAdvanced?.Invoke(currentDialogue);
        }
        else if (HasChoices)
        {
            awaitingChoiceSelection = true;
            DialogueAdvanced?.Invoke(currentDialogue);
        }
        else
        {
            FinishDialogue();
        }
    }

    public List<DialogueChoiceOption> GetChoices()
    {
        if (!HasChoices)
        {
            return new List<DialogueChoiceOption>();
        }

        var options = new List<DialogueChoiceOption>();
        foreach (var option in currentDialogue.DialogueChoice.GetOptions())
        {
            if (option != null)
            {
                options.Add(option);
            }
        }

        return options;
    }

    public string GetChoiceText(int index)
    {
        var options = GetChoices();
        if (index < 0 || index >= options.Count)
        {
            return string.Empty;
        }

        return options[index].GetTextForLanguage(currentLanguageCode);
    }

    public void SelectChoice(int index)
    {
        if (!HasChoices)
        {
            return;
        }

        var options = GetChoices();
        if (index < 0 || index >= options.Count)
        {
            return;
        }

        var selectedOption = options[index];
        if (selectedOption == null)
        {
            return;
        }

        if (selectedOption.NextDialogue != null)
        {
            StartDialogue(selectedOption.NextDialogue, currentLanguageCode);
            return;
        }

        FinishDialogue();
    }

    public void FinishDialogue()
    {
        if (currentDialogue == null)
        {
            return;
        }

        DialogueFinished?.Invoke(currentDialogue);
        currentDialogue = null;
        currentLineIndex = 0;
        awaitingChoiceSelection = false;
    }
}
