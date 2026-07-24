using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcAppearanceSlotAnchor
{
    public NpcAppearanceSlotType slotType;
    public Transform parent;
}

[Serializable]
public class NpcAppearanceMaterialTarget
{
    public NpcColorChannel channel;
    [Min(0)] public int materialIndex;
    public Material referenceMaterial;
}

public class NpcAppearanceColorTarget : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private List<NpcAppearanceMaterialTarget> materialTargets = new List<NpcAppearanceMaterialTarget>();

    public Renderer TargetRenderer => targetRenderer != null ? targetRenderer : GetComponent<Renderer>();
    public IReadOnlyList<NpcAppearanceMaterialTarget> MaterialTargets => materialTargets;

    private void Reset()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
    }
}

public class NpcAppearanceVisual : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private NpcAppearanceCatalog catalog;

    [Header("Anchors")]
    [SerializeField] private Transform baseBodyAnchor;
    [SerializeField] private List<NpcAppearanceSlotAnchor> slotAnchors = new List<NpcAppearanceSlotAnchor>();

    private readonly List<Material> runtimeMaterials = new List<Material>();
    private readonly List<GameObject> spawnedSlotInstances = new List<GameObject>();

    private GameObject currentBaseBodyInstance;
    private NpcAppearanceData currentAppearance;

    public NpcAppearanceCatalog Catalog => catalog;
    public NpcAppearanceData CurrentAppearance => currentAppearance;

    public void ApplyAppearance(NpcAppearanceData appearanceData, NpcAppearanceCatalog overrideCatalog = null)
    {
        var sourceCatalog = overrideCatalog != null ? overrideCatalog : catalog;
        if (sourceCatalog == null)
        {
            Debug.LogWarning($"{nameof(NpcAppearanceVisual)} on {name} is missing an appearance catalog.");
            return;
        }

        currentAppearance = appearanceData;

        RebuildBody(sourceCatalog, appearanceData);
        RebuildSlots(sourceCatalog, appearanceData);
        ApplyColors(sourceCatalog, appearanceData);
    }

    public void ClearAppearance()
    {
        if (currentBaseBodyInstance != null)
        {
            Destroy(currentBaseBodyInstance);
            currentBaseBodyInstance = null;
        }

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

    private void RebuildBody(NpcAppearanceCatalog sourceCatalog, NpcAppearanceData appearanceData)
    {
        if (currentBaseBodyInstance != null)
        {
            Destroy(currentBaseBodyInstance);
            currentBaseBodyInstance = null;
        }

        var baseBody = sourceCatalog.GetBaseBody(appearanceData.bodyType);
        if (baseBody == null || baseBody.baseBodyPrefab == null)
        {
            Debug.LogWarning($"No base body configured for {appearanceData.bodyType} in {sourceCatalog.name}.");
            return;
        }

        var parent = baseBodyAnchor != null ? baseBodyAnchor : transform;
        currentBaseBodyInstance = Instantiate(baseBody.baseBodyPrefab, parent);
        currentBaseBodyInstance.transform.localPosition = Vector3.zero;
        currentBaseBodyInstance.transform.localRotation = Quaternion.identity;
        currentBaseBodyInstance.transform.localScale = Vector3.one;
    }

    private void RebuildSlots(NpcAppearanceCatalog sourceCatalog, NpcAppearanceData appearanceData)
    {
        for (int i = 0; i < spawnedSlotInstances.Count; i++)
        {
            if (spawnedSlotInstances[i] != null)
            {
                Destroy(spawnedSlotInstances[i]);
            }
        }

        spawnedSlotInstances.Clear();

        for (int i = 0; i < slotAnchors.Count; i++)
        {
            var slotType = slotAnchors[i].slotType;
            var slotDefinition = sourceCatalog.GetSlotDefinition(slotType);
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

            var parent = slotAnchors[i].parent != null ? slotAnchors[i].parent : transform;
            var instance = Instantiate(variant.prefab, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            spawnedSlotInstances.Add(instance);

            if (variant.referenceMaterial != null)
            {
                ApplyReferenceMaterialToAllRenderers(instance, variant.referenceMaterial);
            }
        }
    }

    private void ApplyColors(NpcAppearanceCatalog sourceCatalog, NpcAppearanceData appearanceData)
    {
        CleanupRuntimeMaterials();

        var colorTargets = GetComponentsInChildren<NpcAppearanceColorTarget>(true);
        for (int i = 0; i < colorTargets.Length; i++)
        {
            var colorTarget = colorTargets[i];
            var rendererTarget = colorTarget.TargetRenderer;
            if (rendererTarget == null)
            {
                continue;
            }

            var sharedMaterials = rendererTarget.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                continue;
            }

            var runtimeSet = new Material[sharedMaterials.Length];
            Array.Copy(sharedMaterials, runtimeSet, sharedMaterials.Length);

            var materialTargets = colorTarget.MaterialTargets;
            for (int targetIndex = 0; targetIndex < materialTargets.Count; targetIndex++)
            {
                var target = materialTargets[targetIndex];
                if (target == null)
                {
                    continue;
                }

                if (target.materialIndex < 0 || target.materialIndex >= runtimeSet.Length)
                {
                    continue;
                }

                var referenceMaterial = target.referenceMaterial != null
                    ? target.referenceMaterial
                    : sharedMaterials[target.materialIndex];
                if (referenceMaterial == null)
                {
                    continue;
                }

                var runtimeMaterial = new Material(referenceMaterial);
                runtimeMaterials.Add(runtimeMaterial);

                var color = ResolveColor(sourceCatalog, appearanceData, target.channel);
                var binding = sourceCatalog.GetMaterialBinding(target.channel);
                var propertyName = binding != null && !string.IsNullOrWhiteSpace(binding.colorProperty)
                    ? binding.colorProperty
                    : "_BaseColor";

                if (runtimeMaterial.HasProperty(propertyName))
                {
                    runtimeMaterial.SetColor(propertyName, color);
                }
                else if (runtimeMaterial.HasProperty("_Color"))
                {
                    runtimeMaterial.SetColor("_Color", color);
                }

                runtimeSet[target.materialIndex] = runtimeMaterial;
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

    private void ApplyReferenceMaterialToAllRenderers(GameObject root, Material referenceMaterial)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var sharedMaterials = renderers[i].sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0)
            {
                continue;
            }

            var replaced = new Material[sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < replaced.Length; materialIndex++)
            {
                replaced[materialIndex] = referenceMaterial;
            }

            renderers[i].sharedMaterials = replaced;
        }
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