using System;
using System.Collections.Generic;
using UnityEngine;

public enum NpcBodyType
{
    Male = 0,
    Female = 1
}

public enum NpcAppearanceSlotType
{
    UpperBody = 0,
    Mask = 1,
    Hair = 2
}

public enum NpcColorChannel
{
    Skin = 0,
    Hair = 1,
    Undershirt = 2,
    Clothing = 3,
    Shoes = 4,
    Mask = 5
}

[Serializable]
public struct NpcSkinToneSettings
{
    public Color baseColor;
    [Range(-1f, 1f)] public float minDarknessShift;
    [Range(-1f, 1f)] public float maxDarknessShift;
}

[Serializable]
public class NpcPaletteColor
{
    public Color value = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private List<LocalizedText> localizedDescriptions = new List<LocalizedText>();

    public List<LocalizedText> LocalizedDescriptions => localizedDescriptions;

    public NpcPaletteColor(Color value)
    {
        this.value = value;
    }

    public void EnsureLanguageCode(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return;
        }

        if (localizedDescriptions == null)
        {
            localizedDescriptions = new List<LocalizedText>();
        }

        for (int i = 0; i < localizedDescriptions.Count; i++)
        {
            if (localizedDescriptions[i].LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        localizedDescriptions.Add(new LocalizedText(languageCode, string.Empty));
    }

    public void KeepOnlyLanguages(string firstLanguageCode, string secondLanguageCode)
    {
        if (localizedDescriptions == null)
        {
            localizedDescriptions = new List<LocalizedText>();
            return;
        }

        localizedDescriptions.RemoveAll(entry =>
            entry == null ||
            (!entry.LanguageCode.Equals(firstLanguageCode, StringComparison.OrdinalIgnoreCase) &&
             !entry.LanguageCode.Equals(secondLanguageCode, StringComparison.OrdinalIgnoreCase)));
    }

    public void SetDescriptionIfEmpty(string languageCode, string text)
    {
        if (string.IsNullOrWhiteSpace(languageCode) || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (localizedDescriptions == null)
        {
            localizedDescriptions = new List<LocalizedText>();
        }

        for (int i = 0; i < localizedDescriptions.Count; i++)
        {
            if (!localizedDescriptions[i].LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(localizedDescriptions[i].Text))
            {
                localizedDescriptions[i].Text = text;
            }

            return;
        }

        localizedDescriptions.Add(new LocalizedText(languageCode, text));
    }
}

[Serializable]
public struct NpcColorPalette
{
    public NpcColorChannel channel;
    public List<NpcPaletteColor> colors;
}

[Serializable]
[CreateAssetMenu(fileName = "NpcColorPaletteCatalog", menuName = "Scriptable Objects/NPC Color Palette Catalog")]
public class NpcAppearanceCatalog : ScriptableObject
{
    private static readonly string[] descriptionLanguageCodes = { "en", "pt" };

    [Header("Skin Tone")]
    [SerializeField] private NpcSkinToneSettings skinTone = new NpcSkinToneSettings
    {
        baseColor = new Color(0.78f, 0.62f, 0.52f, 1f),
        minDarknessShift = -0.15f,
        maxDarknessShift = 0.2f
    };

    [Header("Palettes")]
    [SerializeField] private List<NpcColorPalette> palettes = new List<NpcColorPalette>();

    public NpcSkinToneSettings SkinTone => skinTone;
    public IReadOnlyList<NpcColorPalette> Palettes => palettes;

    public NpcColorPalette GetPalette(NpcColorChannel channel)
    {
        for (int i = 0; i < palettes.Count; i++)
        {
            if (palettes[i].channel == channel)
            {
                return palettes[i];
            }
        }

        return default;
    }

    private void OnValidate()
    {
        EnsurePaletteColorLocalizations();
    }

    [ContextMenu("Auto-Fill Palette Color Names (EN/PT)")]
    private void AutoFillPaletteColorNames()
    {
        EnsurePaletteColorLocalizations();
    }

    private void EnsurePaletteColorLocalizations()
    {
        if (palettes == null)
        {
            return;
        }

        for (int paletteIndex = 0; paletteIndex < palettes.Count; paletteIndex++)
        {
            NpcColorPalette palette = palettes[paletteIndex];
            if (palette.colors == null)
            {
                continue;
            }

            for (int colorIndex = 0; colorIndex < palette.colors.Count; colorIndex++)
            {
                NpcPaletteColor paletteColor = palette.colors[colorIndex];
                if (paletteColor == null)
                {
                    continue;
                }

                paletteColor.KeepOnlyLanguages("en", "pt");

                for (int codeIndex = 0; codeIndex < descriptionLanguageCodes.Length; codeIndex++)
                {
                    paletteColor.EnsureLanguageCode(descriptionLanguageCodes[codeIndex]);
                }

                string englishName = DescribeColorNameEn(paletteColor.value);
                paletteColor.SetDescriptionIfEmpty("en", englishName);
                paletteColor.SetDescriptionIfEmpty("pt", TranslateColorNameToPt(englishName));
            }

            palettes[paletteIndex] = palette;
        }
    }

    private static string DescribeColorNameEn(Color color)
    {
        Color.RGBToHSV(color, out float hue, out float saturation, out float value);

        if (value <= 0.08f)
        {
            return "Black";
        }

        if (value >= 0.93f && saturation <= 0.08f)
        {
            return "White";
        }

        if (saturation <= 0.12f)
        {
            if (value < 0.35f)
            {
                return "Dark Gray";
            }

            if (value > 0.75f)
            {
                return "Light Gray";
            }

            return "Gray";
        }

        string baseName;

        if (hue < 0.04f || hue >= 0.96f)
        {
            baseName = "Red";
        }
        else if (hue < 0.10f)
        {
            baseName = value < 0.62f ? "Brown" : "Orange";
        }
        else if (hue < 0.17f)
        {
            baseName = value < 0.62f ? "Brown" : "Yellow";
        }
        else if (hue < 0.43f)
        {
            baseName = "Green";
        }
        else if (hue < 0.53f)
        {
            baseName = "Cyan";
        }
        else if (hue < 0.70f)
        {
            baseName = "Blue";
        }
        else if (hue < 0.83f)
        {
            baseName = "Purple";
        }
        else
        {
            baseName = "Pink";
        }

        if (value < 0.35f)
        {
            return $"Dark {baseName}";
        }

        if (value > 0.85f && saturation < 0.45f)
        {
            return $"Light {baseName}";
        }

        return baseName;
    }

    private static string TranslateColorNameToPt(string englishName)
    {
        if (string.IsNullOrWhiteSpace(englishName))
        {
            return string.Empty;
        }

        string[] parts = englishName.Split(' ');
        if (parts.Length == 1)
        {
            return TranslateColorBaseToPt(parts[0]);
        }

        if (parts.Length == 2)
        {
            string prefixPt = TranslatePrefixToPt(parts[0]);
            string basePt = TranslateColorBaseToPt(parts[1]);
            return string.IsNullOrEmpty(prefixPt) ? basePt : $"{prefixPt} {basePt}";
        }

        return englishName;
    }

    private static string TranslatePrefixToPt(string prefix)
    {
        switch (prefix)
        {
            case "Dark":
                return "Escuro";
            case "Light":
                return "Claro";
            default:
                return string.Empty;
        }
    }

    private static string TranslateColorBaseToPt(string baseName)
    {
        switch (baseName)
        {
            case "Black":
                return "Preto";
            case "White":
                return "Branco";
            case "Gray":
                return "Cinza";
            case "Red":
                return "Vermelho";
            case "Orange":
                return "Laranja";
            case "Yellow":
                return "Amarelo";
            case "Green":
                return "Verde";
            case "Cyan":
                return "Ciano";
            case "Blue":
                return "Azul";
            case "Purple":
                return "Roxo";
            case "Pink":
                return "Rosa";
            case "Brown":
                return "Marrom";
            default:
                return baseName;
        }
    }
}

[Serializable]
public struct NpcAppearancePaletteSelection
{
    public NpcColorChannel channel;
    public int colorIndex;
}

[Serializable]
public struct NpcAppearanceSlotSelection
{
    public NpcAppearanceSlotType slotType;
    public int variantIndex;
}

[Serializable]
public struct NpcAppearanceData
{
    public string appearanceId;
    public int generationSeed;
    public NpcBodyType bodyType;
    [Range(0f, 1f)] public float skinDarkness;
    public List<NpcAppearanceSlotSelection> slotSelections;
    public List<NpcAppearancePaletteSelection> paletteSelections;

    public int GetVariantIndex(NpcAppearanceSlotType slotType)
    {
        if (slotSelections == null)
        {
            return -1;
        }

        for (int i = 0; i < slotSelections.Count; i++)
        {
            if (slotSelections[i].slotType == slotType)
            {
                return slotSelections[i].variantIndex;
            }
        }

        return -1;
    }

    public int GetColorIndex(NpcColorChannel channel)
    {
        if (paletteSelections == null)
        {
            return -1;
        }

        for (int i = 0; i < paletteSelections.Count; i++)
        {
            if (paletteSelections[i].channel == channel)
            {
                return paletteSelections[i].colorIndex;
            }
        }

        return -1;
    }
}