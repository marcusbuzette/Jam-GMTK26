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

    public NpcPaletteColor(Color value)
    {
        this.value = value;
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