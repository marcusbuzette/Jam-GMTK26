using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcAppearanceIdentity : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private NpcAppearanceCatalog catalog;
    [SerializeField] private NpcAppearanceVisual worldVisual;
    [SerializeField] private GameObject portraitVisualPrefab;

    [Header("Identity")]
    [SerializeField] private bool generateOnAwake = true;
    [SerializeField] private bool useFixedSeed;
    [SerializeField] private int fixedSeed;
    [SerializeField] private string explicitAppearanceId;

    [Header("Debug")]
    [SerializeField] private NpcAppearanceData currentAppearance;

    public NpcAppearanceCatalog Catalog => catalog;
    public NpcAppearanceData CurrentAppearance => currentAppearance;
    public bool HasAppearance => !string.IsNullOrWhiteSpace(currentAppearance.appearanceId);

    private void Awake()
    {
        if (generateOnAwake)
        {
            EnsureAppearance();
        }
    }

    [ContextMenu("Regenerate Appearance")]
    public void RegenerateAppearance()
    {
        currentAppearance = default;
        EnsureAppearance(true);
    }

    public void EnsureAppearance(bool forceRegenerate = false)
    {
        if (!forceRegenerate && HasAppearance)
        {
            ApplyAppearance();
            return;
        }

        if (catalog == null)
        {
            Debug.LogWarning($"{nameof(NpcAppearanceIdentity)} on {name} is missing an appearance catalog.");
            return;
        }

        var seed = useFixedSeed ? fixedSeed : Guid.NewGuid().GetHashCode();
        currentAppearance = BuildAppearance(seed);
        ApplyAppearance();
    }

    public GameObject CreatePortraitInstance(Transform parent)
    {
        if (portraitVisualPrefab == null || parent == null)
        {
            return null;
        }

        EnsureAppearance();

        var instance = Instantiate(portraitVisualPrefab, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        var portraitVisual = instance.GetComponent<NpcAppearanceVisual>();
        if (portraitVisual == null)
        {
            portraitVisual = instance.GetComponentInChildren<NpcAppearanceVisual>(true);
        }

        if (portraitVisual != null)
        {
            portraitVisual.ApplyAppearance(currentAppearance, catalog);
        }

        return instance;
    }

    private void ApplyAppearance()
    {
        if (worldVisual == null)
        {
            worldVisual = GetComponent<NpcAppearanceVisual>();
        }

        if (worldVisual != null)
        {
            worldVisual.ApplyAppearance(currentAppearance, catalog);
        }
    }

    private NpcAppearanceData BuildAppearance(int seed)
    {
        var random = new System.Random(seed);
        var bodyType = random.Next(0, 2) == 0 ? NpcBodyType.Male : NpcBodyType.Female;

        var slotSelections = new List<NpcAppearanceSlotSelection>();
        for (int i = 0; i < catalog.SlotDefinitions.Count; i++)
        {
            var slotDefinition = catalog.SlotDefinitions[i];
            if (slotDefinition == null)
            {
                continue;
            }

            var validIndexes = GetCompatibleVariantIndexes(slotDefinition, bodyType);
            if (validIndexes.Count == 0)
            {
                continue;
            }

            slotSelections.Add(new NpcAppearanceSlotSelection
            {
                slotType = slotDefinition.slotType,
                variantIndex = validIndexes[random.Next(0, validIndexes.Count)]
            });
        }

        var paletteSelections = new List<NpcAppearancePaletteSelection>();
        for (int i = 0; i < catalog.Palettes.Count; i++)
        {
            var palette = catalog.Palettes[i];
            if (palette.channel == NpcColorChannel.Skin || palette.colors == null || palette.colors.Count == 0)
            {
                continue;
            }

            paletteSelections.Add(new NpcAppearancePaletteSelection
            {
                channel = palette.channel,
                colorIndex = random.Next(0, palette.colors.Count)
            });
        }

        return new NpcAppearanceData
        {
            appearanceId = !string.IsNullOrWhiteSpace(explicitAppearanceId) ? explicitAppearanceId : Guid.NewGuid().ToString("N"),
            generationSeed = seed,
            bodyType = bodyType,
            skinDarkness = (float)random.NextDouble(),
            slotSelections = slotSelections,
            paletteSelections = paletteSelections
        };
    }

    private static List<int> GetCompatibleVariantIndexes(NpcAppearanceSlotDefinition slotDefinition, NpcBodyType bodyType)
    {
        var indexes = new List<int>();
        if (slotDefinition == null || slotDefinition.variants == null)
        {
            return indexes;
        }

        for (int i = 0; i < slotDefinition.variants.Count; i++)
        {
            var variant = slotDefinition.variants[i];
            if (variant == null || variant.prefab == null)
            {
                continue;
            }

            if (variant.supportedBodyTypes == null || variant.supportedBodyTypes.Length == 0)
            {
                indexes.Add(i);
                continue;
            }

            for (int bodyIndex = 0; bodyIndex < variant.supportedBodyTypes.Length; bodyIndex++)
            {
                if (variant.supportedBodyTypes[bodyIndex] == bodyType)
                {
                    indexes.Add(i);
                    break;
                }
            }
        }

        return indexes;
    }
}