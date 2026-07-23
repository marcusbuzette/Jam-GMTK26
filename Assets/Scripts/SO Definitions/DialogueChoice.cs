using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Scriptable Objects/DialogueChoice")]
public class DialogueChoice : ScriptableObject
{
    [Header("Choice A")]
    [SerializeField] private DialogueChoiceOption optionA;

    [Header("Choice B")]
    [SerializeField] private DialogueChoiceOption optionB;

    public DialogueChoiceOption OptionA => optionA;
    public DialogueChoiceOption OptionB => optionB;

    public bool HasOptions => optionA != null && optionB != null;

    public IEnumerable<DialogueChoiceOption> GetOptions()
    {
        if (optionA != null)
        {
            yield return optionA;
        }

        if (optionB != null)
        {
            yield return optionB;
        }
    }
}

[Serializable]
public class DialogueChoiceOption
{
    [SerializeField] private string optionId = "option";
    [SerializeField] private List<LocalizedText> localizedTexts = new List<LocalizedText>();
    [SerializeField] private Dialogue nextDialogue;

    public string OptionId
    {
        get => optionId;
        set => optionId = value;
    }

    public Dialogue NextDialogue
    {
        get => nextDialogue;
        set => nextDialogue = value;
    }

    public List<LocalizedText> LocalizedTexts => localizedTexts;

    public void SetText(string languageCode, string text)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            languageCode = "en";
        }

        for (int i = 0; i < localizedTexts.Count; i++)
        {
            if (localizedTexts[i].LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                localizedTexts[i].Text = text;
                return;
            }
        }

        localizedTexts.Add(new LocalizedText(languageCode, text));
    }

    public string GetTextForLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return localizedTexts.Count > 0 ? localizedTexts[0].Text : string.Empty;
        }

        for (int i = 0; i < localizedTexts.Count; i++)
        {
            if (localizedTexts[i].LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return localizedTexts[i].Text;
            }
        }

        for (int i = 0; i < localizedTexts.Count; i++)
        {
            if (localizedTexts[i].LanguageCode.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                return localizedTexts[i].Text;
            }
        }

        return localizedTexts.Count > 0 ? localizedTexts[0].Text : string.Empty;
    }
}
