using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour {
    private static DialogueUI instance;
    private DialogueManager subscribedManager;
    private EventSystem runtimeEventSystem;
    private static readonly string[] PortraitExpressionBools = {
        nameof(DialoguePortraitExpression.Walk),
        nameof(DialoguePortraitExpression.Run)
    };
    private static readonly string[] PortraitExpressionTriggers = {
        nameof(DialoguePortraitExpression.SpinL),
        nameof(DialoguePortraitExpression.SpinR),
        nameof(DialoguePortraitExpression.Idle1),
        nameof(DialoguePortraitExpression.Idle2),
        nameof(DialoguePortraitExpression.Idle3),
        nameof(DialoguePortraitExpression.Dance1),
        nameof(DialoguePortraitExpression.Dance2),
        nameof(DialoguePortraitExpression.Dance3),
        nameof(DialoguePortraitExpression.Idle_Var1),
        nameof(DialoguePortraitExpression.Idle_Var2),
        nameof(DialoguePortraitExpression.Idle_Var3),
        nameof(DialoguePortraitExpression.Idle_Var4),
        nameof(DialoguePortraitExpression.Idle_Var5),
        nameof(DialoguePortraitExpression.Inter_Question),
        nameof(DialoguePortraitExpression.Inter_Happy),
        nameof(DialoguePortraitExpression.Inter_Exclamation),
        nameof(DialoguePortraitExpression.Inter_Angry),
        nameof(DialoguePortraitExpression.Inter_Sad),
        nameof(DialoguePortraitExpression.Inter_Closed),
        nameof(DialoguePortraitExpression.Inter_Talking)
    };

    public static DialogueUI Instance {
        get {
            if (instance == null) {
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
    [SerializeField, Min(0f)] private float continueDelay = 0.15f;

    private GameObject currentPortraitInstance;
    private UnityEngine.Object currentPortraitSource;
    private bool currentPortraitUsesNpcAppearance;
    private DialoguePortraitExpression currentPortraitExpression;
    private Coroutine continuePromptRoutine;
    private bool continuePromptVisible;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureEventSystem();

        if (skipButton != null) {
            skipButton.onClick.AddListener(HandleSkipPressed);
        }

        if (dialogueTextAnimator == null) {
            dialogueTextAnimator = GetComponent<DialogueTextAnimator>();
        }

        if (dialogueTextAnimator != null) {
            dialogueTextAnimator.TypingFinished += HandleTypingFinished;
        }

        SubscribeToDialogueManager();

        SetDialoguePanelVisible(false);
        SetContinueIndicatorVisible(false);
        SetContainerMode(false);

        if (choiceAButton != null) {
            choiceAButton.onClick.AddListener(() => HandleChoicePressed(0));
        }

        if (choiceBButton != null) {
            choiceBButton.onClick.AddListener(() => HandleChoicePressed(1));
        }

        RefreshDisplay();
    }

    private void OnDestroy() {
        UnsubscribeFromDialogueManager();

        if (dialogueTextAnimator != null) {
            dialogueTextAnimator.TypingFinished -= HandleTypingFinished;
        }
    }

    private void HandleDialogueStarted(Dialogue dialogue) {
        EnsureEventSystem();
        SetCursorForDialogue(true);
        SetDialoguePanelVisible(true);
    }

    private void HandleDialogueChanged(Dialogue dialogue) {
        RefreshDisplay();
    }

    private void HandleDialogueFinished(Dialogue dialogue) {
        ClearChoices();
        ClearPortrait();
        CancelContinuePrompt();
        SetContinueIndicatorVisible(false);
        SetContainerMode(false);
        if (dialogueTextAnimator != null) {
            dialogueTextAnimator.Clear();
        }

        SetDialoguePanelVisible(false);
        SetCursorForDialogue(false);
    }

    private void HandleSkipPressed() {
        HandleAdvanceInput();
    }

    public bool HandleAdvanceInput() {
        var manager = DialogueManager.Instance;
        if (manager == null || !manager.HasDialogue || dialoguePanel == null || !dialoguePanel.activeInHierarchy) {
            return false;
        }

        if (dialogueTextAnimator != null && dialogueTextAnimator.IsTyping) {
            dialogueTextAnimator.SkipTyping();
            return true;
        }

        if (continuePromptVisible) {
            manager.AdvanceLine();
            return true;
        }

        return false;
    }

    private void HandleChoicePressed(int index) {
        var manager = DialogueManager.Instance;
        if (manager == null) {
            return;
        }

        manager.SelectChoice(index);
    }

    private void RefreshDisplay() {
        var manager = DialogueManager.Instance;
        if (manager == null || !manager.HasDialogue) {
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

        if (showChoices && manager.IsAwaitingChoiceSelection) {
            SetContainerMode(true);
            RefreshChoices();
            return;
        }

        SetContainerMode(false);

        var line = manager.GetCurrentLine();
        if (line != null) {
            if (characterNameText != null) {
                characterNameText.text = manager.GetCurrentSpeakerName();
                characterNameText.color = line.Character != null ? line.Character.CharacterAccentColor : new Color32(0x25, 0x24, 0x22, 255);
            }

            if (dialogueTextAnimator != null) {
                dialogueTextAnimator.ShowText(manager.GetCurrentLineText());
            }
        }

        RefreshPortrait();
    }

    private void RefreshChoices() {
        var manager = DialogueManager.Instance;
        var choicesVisible = showChoices && manager != null && manager.IsAwaitingChoiceSelection;

        if (choicesVisible) {
            CancelContinuePrompt();
            SetContinueIndicatorVisible(false);
        }

        if (choiceAText != null && manager != null && choicesVisible) {
            choiceAText.text = manager.GetChoiceText(0);
        }

        if (choiceBText != null && manager != null && choicesVisible) {
            choiceBText.text = manager.GetChoiceText(1);
        }
    }

    private void RefreshPortrait() {
        var manager = DialogueManager.Instance;
        if (manager == null || !manager.HasDialogue) {
            ClearPortrait();
            return;
        }

        var line = manager.GetCurrentLine();
        if (line == null || characterAnchor == null) {
            ClearPortrait();
            return;
        }

        var activeSpeakerAppearance = manager.ActiveSpeakerAppearance;
        if (activeSpeakerAppearance != null && activeSpeakerAppearance.MatchesCharacter(line.Character)) {
            if (!currentPortraitUsesNpcAppearance || currentPortraitSource != activeSpeakerAppearance || currentPortraitInstance == null) {
                ClearPortrait();
                currentPortraitInstance = activeSpeakerAppearance.CreatePortraitInstance(characterAnchor);
                currentPortraitSource = activeSpeakerAppearance;
                currentPortraitUsesNpcAppearance = currentPortraitInstance != null;
                currentPortraitExpression = DialoguePortraitExpression.Neutral;
            }

            if (currentPortraitInstance != null) {
                ApplyPortraitExpressionIfNeeded(currentPortraitInstance, line.PortraitExpression);
                return;
            }
        }

        if (line.Character == null || line.Character.PortraitPrefab == null) {
            ClearPortrait();
            return;
        }

        if (currentPortraitUsesNpcAppearance || currentPortraitSource != line.Character.PortraitPrefab || currentPortraitInstance == null) {
            ClearPortrait();
            currentPortraitInstance = Instantiate(line.Character.PortraitPrefab, characterAnchor);
            currentPortraitInstance.transform.localPosition = Vector3.zero;
            currentPortraitInstance.transform.localRotation = Quaternion.identity;
            currentPortraitSource = line.Character.PortraitPrefab;
            currentPortraitUsesNpcAppearance = false;
            currentPortraitExpression = DialoguePortraitExpression.Neutral;
        }

        ApplyPortraitExpressionIfNeeded(currentPortraitInstance, line.PortraitExpression);
    }

    private void ApplyPortraitExpressionIfNeeded(GameObject portraitInstance, DialoguePortraitExpression expression) {
        if (currentPortraitExpression == expression) {
            return;
        }

        ApplyPortraitExpression(portraitInstance, expression);
        currentPortraitExpression = expression;
    }

    private void ApplyPortraitExpression(GameObject portraitInstance, DialoguePortraitExpression expression) {
        if (portraitInstance == null) {
            return;
        }

        var portraitAnimator = portraitInstance.GetComponentInChildren<Animator>(true);
        if (portraitAnimator == null) {
            return;
        }

        for (int i = 0; i < PortraitExpressionBools.Length; i++) {
            if (portraitAnimator.HasParameterOfType(PortraitExpressionBools[i], AnimatorControllerParameterType.Bool)) {
                portraitAnimator.SetBool(PortraitExpressionBools[i], false);
            }
        }

        for (int i = 0; i < PortraitExpressionTriggers.Length; i++) {
            if (portraitAnimator.HasParameterOfType(PortraitExpressionTriggers[i], AnimatorControllerParameterType.Trigger)) {
                portraitAnimator.ResetTrigger(PortraitExpressionTriggers[i]);
            }
        }

        if (expression == DialoguePortraitExpression.Neutral) {
            return;
        }

        var parameterName = expression.ToString();
        if (portraitAnimator.HasParameterOfType(parameterName, AnimatorControllerParameterType.Bool)) {
            portraitAnimator.SetBool(parameterName, true);
            return;
        }

        if (portraitAnimator.HasParameterOfType(parameterName, AnimatorControllerParameterType.Trigger)) {
            portraitAnimator.SetTrigger(parameterName);
        }
    }

    private void ClearPortrait() {
        if (currentPortraitInstance != null) {
            Destroy(currentPortraitInstance);
            currentPortraitInstance = null;
        }

        currentPortraitSource = null;
        currentPortraitUsesNpcAppearance = false;
        currentPortraitExpression = DialoguePortraitExpression.Neutral;
    }

    private void HandleTypingFinished() {
        CancelContinuePrompt();
        continuePromptRoutine = StartCoroutine(ShowContinuePromptAfterDelayRoutine());
    }

    private IEnumerator ShowContinuePromptAfterDelayRoutine() {
        yield return new WaitForSeconds(continueDelay);

        continuePromptRoutine = null;
        if (DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue && !DialogueManager.Instance.IsAwaitingChoiceSelection) {
            SetContinueIndicatorVisible(true);
        }
    }

    private void CancelContinuePrompt() {
        if (continuePromptRoutine != null) {
            StopCoroutine(continuePromptRoutine);
            continuePromptRoutine = null;
        }
    }

    private void SubscribeToDialogueManager() {
        var manager = DialogueManager.Instance;
        if (manager == null || manager == subscribedManager) {
            return;
        }

        UnsubscribeFromDialogueManager();

        subscribedManager = manager;
        subscribedManager.DialogueStarted += HandleDialogueStarted;
        subscribedManager.DialogueAdvanced += HandleDialogueChanged;
        subscribedManager.DialogueFinished += HandleDialogueFinished;
    }

    private void UnsubscribeFromDialogueManager() {
        if (subscribedManager == null) {
            return;
        }

        subscribedManager.DialogueStarted -= HandleDialogueStarted;
        subscribedManager.DialogueAdvanced -= HandleDialogueChanged;
        subscribedManager.DialogueFinished -= HandleDialogueFinished;
        subscribedManager = null;
    }

    private void EnsureEventSystem() {
        runtimeEventSystem = EventSystem.current;
        if (runtimeEventSystem == null) {
            var eventSystemObject = new GameObject("EventSystem");
            runtimeEventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (runtimeEventSystem.GetComponent<InputSystemUIInputModule>() == null) {
            var existingStandaloneModule = runtimeEventSystem.GetComponent<StandaloneInputModule>();
            if (existingStandaloneModule != null) {
                Destroy(existingStandaloneModule);
            }

            runtimeEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void SetCursorForDialogue(bool dialogueActive) {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!dialogueActive && DialogueManager.Instance != null && DialogueManager.Instance.HasDialogue) {
            return;
        }
    }

    private void SetDialoguePanelVisible(bool visible) {
        if (dialoguePanel != null) {
            dialoguePanel.SetActive(visible);
        }
    }

    private void SetContinueIndicatorVisible(bool visible) {
        continuePromptVisible = visible;
        if (continueIndicator != null) {
            continueIndicator.SetActive(visible);
        }
    }

    private void SetContainerMode(bool showingChoices) {
        if (dialogueLineContainer != null) {
            dialogueLineContainer.SetActive(!showingChoices);
        }

        if (choicesContainer != null) {
            choicesContainer.SetActive(showingChoices);
        }
    }

    private void ClearChoices() {
        SetContainerMode(false);
    }
}

public static class AnimatorParameterExtensions
{
    public static bool HasParameterOfType(this Animator animator, string parameterName, AnimatorControllerParameterType parameterType)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
        {
            return false;
        }

        var parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == parameterType && parameters[i].name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}
