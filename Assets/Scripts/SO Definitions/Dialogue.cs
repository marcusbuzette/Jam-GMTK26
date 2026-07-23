using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class Dialogue : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string conversationId = "conversation";
    [SerializeField] private string displayName = "Dialogue";

    [Header("Conversation")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Choices")]
    [SerializeField] private DialogueChoice dialogueChoice;

    public string ConversationId => conversationId;
    public string DisplayName => displayName;
    public IReadOnlyList<DialogueLine> Lines => lines;
    public DialogueChoice DialogueChoice => dialogueChoice;

    public DialogueLine GetLine(int index)
    {
        if (index < 0 || index >= lines.Count)
        {
            return null;
        }

        return lines[index];
    }

    public string GetLineText(int index, string languageCode = null)
    {
        var line = GetLine(index);
        if (line == null)
        {
            return string.Empty;
        }

        return line.GetTextForLanguage(languageCode ?? GetCurrentLanguageCode());
    }

    public void AddLine(CharacterDefinition character, string text, string languageCode = "en")
    {
        var line = new DialogueLine();
        line.Character = character;
        line.SetText(languageCode, text);
        lines.Add(line);
    }

    public static string GetCurrentLanguageCode()
    {
        return GetLanguageCode(Application.systemLanguage);
    }

    public static string GetLanguageCode(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Portuguese:
                return "pt";
            case SystemLanguage.Spanish:
                return "es";
            case SystemLanguage.French:
                return "fr";
            case SystemLanguage.German:
                return "de";
            case SystemLanguage.Italian:
                return "it";
            case SystemLanguage.Japanese:
                return "ja";
            case SystemLanguage.Korean:
                return "ko";
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                return "zh";
            case SystemLanguage.Russian:
                return "ru";
            case SystemLanguage.Arabic:
                return "ar";
            default:
                return "en";
        }
    }
}

[Serializable]
public class DialogueLine
{
    [SerializeField] private CharacterDefinition character;
    [SerializeField] private List<LocalizedText> localizedTexts = new List<LocalizedText>();

    public CharacterDefinition Character
    {
        get => character;
        set => character = value;
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

    public string GetSpeakerName()
    {
        return character != null ? character.CharacterName : "Narrator";
    }
}

[Serializable]
public class LocalizedText
{
    [SerializeField] private string languageCode = "en";
    [SerializeField] private string text = string.Empty;

    public LocalizedText()
    {
    }

    public LocalizedText(string languageCode, string text)
    {
        this.languageCode = languageCode;
        this.text = text;
    }

    public string LanguageCode
    {
        get => languageCode;
        set => languageCode = value;
    }

    public string Text
    {
        get => text;
        set => text = value;
    }
}
