using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcAppearanceMaterialTarget
{
    public NpcColorChannel channel;
    [Min(0)] public int materialIndex;
    public string colorProperty = "_BaseColor";
}

[Serializable]
public class NpcBaseBodyOption
{
    public NpcBodyType bodyType;
    public GameObject baseBodyPrefab;
}

[Serializable]
public class NpcAppearanceVariant
{
    public GameObject prefab;
    public NpcBodyType[] supportedBodyTypes = Array.Empty<NpcBodyType>();

    public string Identifier => prefab != null ? prefab.name : string.Empty;
}

[Serializable]
public class NpcAppearanceSlotDefinition
{
    public NpcAppearanceSlotType slotType;
    public List<NpcAppearanceVariant> variants = new List<NpcAppearanceVariant>();
}

public class NpcAppearanceColorTarget : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [ReadOnlyInInspector]
    [SerializeField] private List<NpcAppearanceMaterialTarget> materialTargets = new List<NpcAppearanceMaterialTarget>();

    public Renderer TargetRenderer => targetRenderer != null ? targetRenderer : GetComponent<Renderer>();
    public IReadOnlyList<NpcAppearanceMaterialTarget> MaterialTargets => materialTargets;

    private void Reset()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        AutoConfigureMaterialTargets();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        AutoConfigureMaterialTargets();
    }

    public IReadOnlyList<NpcAppearanceMaterialTarget> GetEffectiveMaterialTargets()
    {
        AutoConfigureMaterialTargets();
        return materialTargets;
    }

    private void AutoConfigureMaterialTargets()
    {
        var renderer = TargetRenderer;
        if (renderer == null)
        {
            return;
        }

        var sharedMaterials = renderer.sharedMaterials;
        if (sharedMaterials == null || sharedMaterials.Length == 0)
        {
            return;
        }

        var detectedTargets = new List<NpcAppearanceMaterialTarget>();
        for (int i = 0; i < sharedMaterials.Length; i++)
        {
            var material = sharedMaterials[i];
            if (material == null)
            {
                continue;
            }

            if (!NpcAppearanceVisual.TryInferChannelFromMaterial(material.name, out var channel))
            {
                continue;
            }

            var colorProperty = material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            detectedTargets.Add(new NpcAppearanceMaterialTarget
            {
                channel = channel,
                materialIndex = i,
                colorProperty = colorProperty
            });
        }

        materialTargets = detectedTargets;
    }
}

public class NpcAppearanceVisual : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private NpcAppearanceCatalog catalog;

    [Header("Module Prefabs")]
    [SerializeField] private List<NpcBaseBodyOption> baseBodies = new List<NpcBaseBodyOption>();
    [SerializeField] private List<NpcAppearanceSlotDefinition> slotDefinitions = new List<NpcAppearanceSlotDefinition>();

    [Header("Anchors")]
    [SerializeField] private Transform baseBodyAnchor;

    private readonly List<Material> runtimeMaterials = new List<Material>();
    private readonly List<GameObject> spawnedSlotInstances = new List<GameObject>();

    private GameObject currentBaseBodyInstance;
    private NpcAppearanceData currentAppearance;
    private bool ownsBaseBodyInstance;

    public NpcAppearanceCatalog Catalog => catalog;
    public IReadOnlyList<NpcBaseBodyOption> BaseBodies => baseBodies;
    public IReadOnlyList<NpcAppearanceSlotDefinition> SlotDefinitions => slotDefinitions;
    public NpcAppearanceData CurrentAppearance => currentAppearance;

    public Animator GetActiveBodyAnimator()
    {
        if (currentBaseBodyInstance == null)
        {
            return null;
        }

        return currentBaseBodyInstance.GetComponentInChildren<Animator>(true);
    }

    public static bool TryInferChannelFromMaterial(string materialName, out NpcColorChannel channel)
    {
        var normalizedName = NormalizeMaterialName(materialName);

        if (normalizedName.Contains("skin"))
        {
            channel = NpcColorChannel.Skin;
            return true;
        }

        if (normalizedName.Contains("cabelo") || normalizedName.Contains("hair"))
        {
            channel = NpcColorChannel.Hair;
            return true;
        }

        if (normalizedName.Contains("undershirt") || normalizedName.Contains("shirtbeneath") || normalizedName.Contains("inner"))
        {
            channel = NpcColorChannel.Undershirt;
            return true;
        }

        if (normalizedName.Contains("clothespants") || normalizedName.Contains("pants") || normalizedName.Contains("shirt") || normalizedName.Contains("clothes") || normalizedName.Contains("coat") || normalizedName.Contains("suit"))
        {
            channel = NpcColorChannel.Clothing;
            return true;
        }

        if (normalizedName.Contains("shoe"))
        {
            channel = NpcColorChannel.Shoes;
            return true;
        }

        if (normalizedName.Contains("mask"))
        {
            channel = NpcColorChannel.Mask;
            return true;
        }

        channel = default;
        return false;
    }

    public void ApplyAppearance(NpcAppearanceData appearanceData, NpcAppearanceCatalog overrideCatalog = null)
    {
        var sourceCatalog = overrideCatalog != null ? overrideCatalog : catalog;
        if (sourceCatalog == null)
        {
            Debug.LogWarning($"{nameof(NpcAppearanceVisual)} on {name} is missing an appearance catalog.");
            return;
        }

        currentAppearance = appearanceData;

        RebuildBody(appearanceData);
        RebuildSlots(appearanceData);
        ApplyColors(sourceCatalog, appearanceData);
    }

    public void ClearAppearance()
    {
        if (currentBaseBodyInstance != null && ownsBaseBodyInstance)
        {
            Destroy(currentBaseBodyInstance);
        }

        currentBaseBodyInstance = null;
        ownsBaseBodyInstance = false;

        for (int i = 0; i < spawnedSlotInstances.Count; i++)
        {
            if (spawnedSlotInstances[i] != null)
            {
                Destroy(spawnedSlotInstances[i]);
            }
        }

        spawnedSlotInstances.Clear();
        CleanupRuntimeMaterials();
    }

    private void OnDestroy()
    {
        CleanupRuntimeMaterials();
    }

    public bool HasBaseBody(NpcBodyType bodyType)
    {
        return GetBaseBody(bodyType) != null;
    }

    public NpcBaseBodyOption GetBaseBody(NpcBodyType bodyType)
    {
        for (int i = 0; i < baseBodies.Count; i++)
        {
            if (baseBodies[i] != null && baseBodies[i].bodyType == bodyType)
            {
                return baseBodies[i];
            }
        }

        return null;
    }

    public NpcAppearanceSlotDefinition GetSlotDefinition(NpcAppearanceSlotType slotType)
    {
        for (int i = 0; i < slotDefinitions.Count; i++)
        {
            if (slotDefinitions[i] != null && slotDefinitions[i].slotType == slotType)
            {
                return slotDefinitions[i];
            }
        }

        return null;
    }

    private void RebuildBody(NpcAppearanceData appearanceData)
    {
        if (currentBaseBodyInstance != null && ownsBaseBodyInstance)
        {
            Destroy(currentBaseBodyInstance);
        }

        currentBaseBodyInstance = null;
        ownsBaseBodyInstance = false;

        var baseBody = GetBaseBody(appearanceData.bodyType);
        if (baseBody == null || baseBody.baseBodyPrefab == null)
        {
            Debug.LogWarning($"No base body configured for {appearanceData.bodyType} on {name}.");
            return;
        }

        var authoredInstance = FindAuthoredBaseBodyInstance(baseBody.baseBodyPrefab);
        if (authoredInstance != null)
        {
            currentBaseBodyInstance = authoredInstance;
            currentBaseBodyInstance.SetActive(true);
            return;
        }

        var parent = baseBodyAnchor != null ? baseBodyAnchor : transform;
        currentBaseBodyInstance = Instantiate(baseBody.baseBodyPrefab, parent);
        currentBaseBodyInstance.transform.localPosition = Vector3.zero;
        currentBaseBodyInstance.transform.localRotation = Quaternion.identity;
        currentBaseBodyInstance.transform.localScale = Vector3.one;
        ownsBaseBodyInstance = true;
    }

    private void RebuildSlots(NpcAppearanceData appearanceData)
    {
        if (currentBaseBodyInstance == null)
        {
            return;
        }

        for (int i = 0; i < spawnedSlotInstances.Count; i++)
        {
            if (spawnedSlotInstances[i] != null)
            {
                Destroy(spawnedSlotInstances[i]);
            }
        }

        spawnedSlotInstances.Clear();
        DisableRegisteredVariants();

        for (int i = 0; i < slotDefinitions.Count; i++)
        {
            var slotDefinition = slotDefinitions[i];
            if (slotDefinition == null)
            {
                continue;
            }

            var slotType = slotDefinition.slotType;
            if (slotDefinition == null || slotDefinition.variants == null || slotDefinition.variants.Count == 0)
            {
                continue;
            }

            var variantIndex = appearanceData.GetVariantIndex(slotType);
            if (variantIndex < 0 || variantIndex >= slotDefinition.variants.Count)
            {
                continue;
            }

            var variant = slotDefinition.variants[variantIndex];
            if (variant == null || variant.prefab == null)
            {
                continue;
            }

            if (TryResolveVariantInstance(variant, out var existingVariant))
            {
                existingVariant.gameObject.SetActive(true);
                continue;
            }

            var instance = Instantiate(variant.prefab, currentBaseBodyInstance.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);
            spawnedSlotInstances.Add(instance);
        }
    }

    private void ApplyColors(NpcAppearanceCatalog sourceCatalog, NpcAppearanceData appearanceData)
    {
        CleanupRuntimeMaterials();

        if (currentBaseBodyInstance == null)
        {
            return;
        }

        var renderers = currentBaseBodyInstance.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var rendererTarget = renderers[i];
            var sharedMaterials = rendererTarget.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                continue;
            }

            var runtimeSet = new Material[sharedMaterials.Length];
            Array.Copy(sharedMaterials, runtimeSet, sharedMaterials.Length);

            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                var referenceMaterial = sharedMaterials[materialIndex];
                if (referenceMaterial == null)
                {
                    continue;
                }

                if (!TryInferChannelFromMaterial(referenceMaterial.name, out var channel))
                {
                    continue;
                }

                var runtimeMaterial = new Material(referenceMaterial);
                runtimeMaterials.Add(runtimeMaterial);

                var color = ResolveColor(sourceCatalog, appearanceData, channel);
                var propertyName = referenceMaterial.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";

                if (runtimeMaterial.HasProperty(propertyName))
                {
                    runtimeMaterial.SetColor(propertyName, color);
                }
                else if (runtimeMaterial.HasProperty("_Color"))
                {
                    runtimeMaterial.SetColor("_Color", color);
                }

                runtimeSet[materialIndex] = runtimeMaterial;
            }

            rendererTarget.materials = runtimeSet;
        }
    }

    private Color ResolveColor(NpcAppearanceCatalog sourceCatalog, NpcAppearanceData appearanceData, NpcColorChannel channel)
    {
        if (channel == NpcColorChannel.Skin)
        {
            return ResolveSkinColor(sourceCatalog.SkinTone, appearanceData.skinDarkness);
        }

        var palette = sourceCatalog.GetPalette(channel);
        if (palette.colors == null || palette.colors.Count == 0)
        {
            return Color.white;
        }

        var colorIndex = appearanceData.GetColorIndex(channel);
        if (colorIndex < 0 || colorIndex >= palette.colors.Count)
        {
            colorIndex = 0;
        }

        return palette.colors[colorIndex].value;
    }

    private static Color ResolveSkinColor(NpcSkinToneSettings settings, float normalizedDarkness)
    {
        Color.RGBToHSV(settings.baseColor, out float hue, out float saturation, out float value);
        var darknessShift = Mathf.Lerp(settings.minDarknessShift, settings.maxDarknessShift, Mathf.Clamp01(normalizedDarkness));
        var adjustedValue = Mathf.Clamp01(value - darknessShift);
        return Color.HSVToRGB(hue, saturation, adjustedValue);
    }

    private static string NormalizeMaterialName(string materialName)
    {
        if (string.IsNullOrWhiteSpace(materialName))
        {
            return string.Empty;
        }

        var instanceSuffixIndex = materialName.IndexOf(" (Instance)", StringComparison.OrdinalIgnoreCase);
        if (instanceSuffixIndex >= 0)
        {
            materialName = materialName.Substring(0, instanceSuffixIndex);
        }

        return materialName.Replace("_", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
    }

    private GameObject FindAuthoredBaseBodyInstance(GameObject baseBodyPrefab)
    {
        if (baseBodyPrefab == null)
        {
            return null;
        }

        if (baseBodyAnchor != null && string.Equals(baseBodyAnchor.name, baseBodyPrefab.name, StringComparison.Ordinal))
        {
            return baseBodyAnchor.gameObject;
        }

        var searchRoot = baseBodyAnchor != null ? baseBodyAnchor : transform;
        var match = FindDeepChildByName(searchRoot, baseBodyPrefab.name);
        return match != null ? match.gameObject : null;
    }

    private void DisableRegisteredVariants()
    {
        if (currentBaseBodyInstance == null)
        {
            return;
        }

        for (int slotIndex = 0; slotIndex < slotDefinitions.Count; slotIndex++)
        {
            var slotDefinition = slotDefinitions[slotIndex];
            if (slotDefinition == null || slotDefinition.variants == null)
            {
                continue;
            }

            for (int variantIndex = 0; variantIndex < slotDefinition.variants.Count; variantIndex++)
            {
                var variant = slotDefinition.variants[variantIndex];
                if (variant == null || variant.prefab == null)
                {
                    continue;
                }

                if (TryResolveVariantInstance(variant, out var existingVariant))
                {
                    existingVariant.gameObject.SetActive(false);
                }
            }
        }
    }

    private bool TryResolveVariantInstance(NpcAppearanceVariant variant, out Transform match)
    {
        match = null;
        if (variant == null || variant.prefab == null || currentBaseBodyInstance == null)
        {
            return false;
        }

        var variantObject = variant.prefab;
        if (variantObject != null && variantObject.scene.IsValid())
        {
            var variantTransform = variantObject.transform;
            if (variantTransform.IsChildOf(currentBaseBodyInstance.transform))
            {
                match = variantTransform;
                return true;
            }
        }

        return TryFindVariantInstance(currentBaseBodyInstance.transform, variant.Identifier, out match);
    }

    private static bool TryFindVariantInstance(Transform root, string variantName, out Transform match)
    {
        match = FindDeepChildByName(root, variantName);
        return match != null;
    }

    private static Transform FindDeepChildByName(Transform root, string targetName)
    {
        if (root == null || string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        if (string.Equals(root.name, targetName, StringComparison.Ordinal))
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            var match = FindDeepChildByName(root.GetChild(i), targetName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private void CleanupRuntimeMaterials()
    {
        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            if (runtimeMaterials[i] != null)
            {
                Destroy(runtimeMaterials[i]);
            }
        }

        runtimeMaterials.Clear();
    }
}