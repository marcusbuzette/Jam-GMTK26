using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    private static DialogueUI instance;

    public static DialogueUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<DialogueUI>();
            }

            return instance;
        }
    }

    [Header("References")]
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject dialogueLineContainer;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private GameObject continueIndicator;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private DialogueTextAnimator dialogueTextAnimator;
    [SerializeField] private TextMeshProUGUI choiceAText;
    [SerializeField] private TextMeshProUGUI choiceBText;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private Transform characterAnchor;

    [Header("Display")]
    [SerializeField] private bool showChoices = true;
    [SerializeField, Min(0f)] private float skipGracePeriod = 0.2f;
    [SerializeField, Min(0f)] private float continueDelay = 0.15f;

    private GameObject currentPortraitInstance;
    private Coroutine pendingAdvanceRoutine;
    private Coroutine continuePromptRoutine;
    private bool hasPendingAdvance;
    private bool continuePromptVisible;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(HandleSkipPressed);
        }

        if (dialogueTextAnimator == null)
        {
            dialogueTextAnimator = GetComponent<DialogueTextAnimator>();
        }

        if (dialogueTextAnimator != null)
        {
            dialogueTextAnimator.TypingFinished += HandleTypingFinished;
        }

        SetDialoguePanelVisible(false);
        SetContinueIndicatorVisible(false);
        SetContainerMode(false);

        if (choiceAButton != null)
        {
            choiceAButton.onClick.AddListener(() => HandleChoicePressed(0));
        }

        if (choiceBButton != null)
        {
            choiceBButton.onClick.AddListener(() => HandleChoicePressed(1));
        }

        RefreshDisplay();
    }

    private void OnEnable()
    {
        var manager = DialogueManager.Instance;
        if (manager != null)
        {
            manager.DialogueStarted += HandleDialogueChanged;
            manager.DialogueAdvanced += HandleDialogueChanged;
            manager.DialogueFinished += HandleDialogueFinished;
        }
    }

    private void OnDisable()
    {
        var manager = DialogueManager.Instance;
        if (manager != null)
        {
            manager.DialogueStarted -= HandleDialogueChanged;
            manager.DialogueAdvanced -= HandleDialogueChanged;
            manager.DialogueFinished -= HandleDialogueFinished;
        }

        if (dialogueTextAnimator != null)
        {
            dialogueTextAnimator.TypingFinished -= HandleTypingFinished;
        }
    }

    private void HandleDialogueChanged(Dialogue dialogue)
    {
        CancelPendingAdvance();
        RefreshDisplay();
    }

    private void HandleDialogueFinished(Dialogue dialogue)
    {
        CancelPendingAdvance();
        ClearChoices();
        ClearPortrait();
        CancelContinuePrompt();
        SetContinueIndicatorVisible(false);
        SetContainerMode(false);
        if (dialogueTextAnimator != null)
        {
            dialogueTextAnimator.Clear();
        }

        SetDialoguePanelVisible(false);
    }

    private void HandleSkipPressed()
    {
        var manager = DialogueManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (dialogueTextAnimator != null && dialogueTextAnimator.IsTyping)
        {
            dialogueTextAnimator.SkipTyping();
            return;
        }

        if (continuePromptVisible)
        {
            manager.AdvanceLine();
            return;
        }
    }

    private void HandleChoicePressed(int index)
    {
        var manager = DialogueManager.Instance;
        if (manager == null)
        {
            return;
        }

        manager.SelectChoice(index);
    }

    private void RefreshDisplay()
    {
        var manager = DialogueManager.Instance;
        if (manager == null || !manager.HasDialogue)
        {
            CancelContinuePrompt();
            SetContinueIndicatorVisible(false);
            ClearChoices();
            ClearPortrait();
            SetDialoguePanelVisible(false);
            return;
        }

        CancelContinuePrompt();
        SetContinueIndicatorVisible(false);

        SetDialoguePanelVisible(true);

        if (showChoices && manager.IsAwaitingChoiceSelection)
        {
            SetContainerMode(true);
            RefreshChoices();
            return;
        }

        SetContainerMode(false);

        var line = manager.GetCurrentLine();
        if (line != null)
        {
            if (characterNameText != null)
            {
                characterNameText.text = manager.GetCurrentSpeakerName();
                characterNameText.color = line.Character != null ? line.Character.CharacterAccentColor : Color.white;
            }

            if (dialogueTextAnimator != null)
            {
                dialogueTextAnimator.ShowText(manager.GetCurrentLineText());
            }
        }

        RefreshPortrait();
    }

    private void RefreshChoices()
    {
        var manager = DialogueManager.Instance;
        var choicesVisible = showChoices && manager != null && manager.IsAwaitingChoiceSelection;

        if (choicesVisible)
        {
            CancelContinuePrompt();
            SetContinueIndicatorVisible(false);
        }

        if (choiceAText != null && manager != null && choicesVisible)
        {
            choiceAText.text = manager.GetChoiceText(0);
        }

        if (choiceBText != null && manager != null && choicesVisible)
        {
            choiceBText.text = manager.GetChoiceText(1);
        }
    }

    private void RefreshPortrait()
    {
        ClearPortrait();

        var manager = DialogueManager.Instance;
        if (manager == null || !manager.HasDialogue)
        {
            return;
        }

        var line = manager.GetCurrentLine();
        if (line == null || line.Character == null || line.Character.PortraitPrefab == null || characterAnchor == null)
        {
            return;
        }

        currentPortraitInstance = Instantiate(line.Character.PortraitPrefab, characterAnchor);
        currentPortraitInstance.transform.localPosition = Vector3.zero;
        currentPortraitInstance.transform.localRotation = Quaternion.identity;
    }

    private void ClearPortrait()
    {
        if (currentPortraitInstance != null)
        {
            Destroy(currentPortraitInstance);
            currentPortraitInstance = null;
        }
    }

    private void HandleTypingFinished()
    {
        CancelContinuePrompt();
        continuePromptRoutine = StartCoroutine(ShowContinuePromptAfterDelayRoutine());
    }

    private IEnumerator ShowContinuePromptAfterDelayRoutine()
    {
        yield return new WaitForSeconds(continueDelay);

        continuePromptRoutine = null;
        if (DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue && !DialogueManager.Instance.IsAwaitingChoiceSelection)
        {
            SetContinueIndicatorVisible(true);
        }
    }

    private void CancelContinuePrompt()
    {
        if (continuePromptRoutine != null)
        {
            StopCoroutine(continuePromptRoutine);
            continuePromptRoutine = null;
        }
    }

    private void CancelPendingAdvance()
    {
        if (pendingAdvanceRoutine != null)
        {
            StopCoroutine(pendingAdvanceRoutine);
            pendingAdvanceRoutine = null;
        }

        hasPendingAdvance = false;
    }

    private void SetDialoguePanelVisible(bool visible)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(visible);
        }
    }

    private void SetContinueIndicatorVisible(bool visible)
    {
        continuePromptVisible = visible;
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(visible);
        }
    }

    private void SetContainerMode(bool showingChoices)
    {
        if (dialogueLineContainer != null)
        {
            dialogueLineContainer.SetActive(!showingChoices);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(showingChoices);
        }
    }

    private void ClearChoices()
    {
        SetContainerMode(false);
    }
}
