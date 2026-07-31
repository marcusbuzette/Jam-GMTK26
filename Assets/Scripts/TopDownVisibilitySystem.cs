using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownVisibilitySystem : MonoBehaviour {
    private enum ClipMode {
        None,
        Hole
    }

    private sealed class RendererState {
        public Material[] OriginalSharedMaterials;
        public Material[] HoleSharedMaterials;
        public ClipMode CurrentMode;
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Shader visibilityShader;

    [Header("Occluder Hole")]
    [SerializeField, CompleteLayerMaskAttribute] private LayerMask obstructionMask = ~0;
    [SerializeField, Min(0.01f)] private float castRadius = 0.35f;
    [SerializeField, Min(0.01f)] private float holeRadius = 1.45f;
    [SerializeField, Min(0f)] private float holeSoftness = 0.35f;
    [SerializeField, Min(1f)] private float minimumHoleRadiusPixels = 18f;
    [SerializeField, Min(1f)] private float minimumHoleSoftnessPixels = 4f;

    [Header("Target Offset")]
    [SerializeField] private Vector3 targetWorldOffset = new Vector3(0f, 0.9f, 0f);

    private Camera cachedCamera;
    private bool missingTargetWarningLogged;
    private readonly Dictionary<Renderer, RendererState> rendererStates = new Dictionary<Renderer, RendererState>();
    private readonly HashSet<Renderer> activeOccluders = new HashSet<Renderer>();
    private readonly HashSet<Renderer> nextOccluders = new HashSet<Renderer>();

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int HoleCenterViewportId = Shader.PropertyToID("_HoleCenterViewport");
    private static readonly int HoleRadiusPixelsId = Shader.PropertyToID("_HoleRadiusPixels");
    private static readonly int HoleSoftnessPixelsId = Shader.PropertyToID("_HoleSoftnessPixels");
    private static readonly int HoleEnabledId = Shader.PropertyToID("_HoleEnabled");
    private static readonly int PlaneEnabledId = Shader.PropertyToID("_PlaneEnabled");

    private void Awake() {
        cachedCamera = GetComponent<Camera>();

        if (visibilityShader == null) {
            visibilityShader = Shader.Find("Custom/URPVisibilityClip");
        }
    }

    private void Start() {
        AssignDefaultReferences();
    }

    private void OnEnable() {
        AssignDefaultReferences();
    }

    private void AssignDefaultReferences() {
        if (target != null) {
            return;
        }

        if (LevelManager.Instance != null && LevelManager.Instance.PlayerTransform != null) {
            target = LevelManager.Instance.PlayerTransform;
            missingTargetWarningLogged = false;
            return;
        }

        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
        if (playerMovement != null) {
            target = playerMovement.transform;
            missingTargetWarningLogged = false;
            return;
        }

        if (!missingTargetWarningLogged) {
            Debug.LogWarning("TopDownVisibilitySystem: target is not assigned yet. Waiting for LevelManager.PlayerTransform or a PlayerMovement instance.");
            missingTargetWarningLogged = true;
        }
    }

    private void OnDisable() {
        RestoreAllRenderers();
    }

    private void LateUpdate() {
        if (visibilityShader == null) {
            return;
        }

        if (target == null) {
            AssignDefaultReferences();
            if (target == null) {
                ClearActiveOccluders();
                return;
            }
        }

        RefreshGeneratedMaterialSettings();

        Vector3 targetPosition = target.position + targetWorldOffset;
        Vector3 cameraPosition = cachedCamera.transform.position;
        Vector3 cameraToTarget = targetPosition - cameraPosition;
        float distanceToTarget = cameraToTarget.magnitude;

        if (distanceToTarget <= Mathf.Epsilon) {
            return;
        }

        Vector3 directionToTarget = cameraToTarget / distanceToTarget;

        UpdateOccluders(cameraPosition, directionToTarget, distanceToTarget, targetPosition);
    }

    private void UpdateOccluders(Vector3 cameraPosition, Vector3 directionToTarget, float distanceToTarget, Vector3 targetPosition) {
        nextOccluders.Clear();

        RaycastHit[] hits = Physics.SphereCastAll(
            cameraPosition,
            castRadius,
            directionToTarget,
            distanceToTarget,
            obstructionMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++) {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null) {
                continue;
            }

            if (target != null && hitCollider.transform.IsChildOf(target)) {
                continue;
            }

            Renderer hitRenderer = hitCollider.GetComponentInParent<Renderer>();
            if (hitRenderer == null) {
                continue;
            }

            nextOccluders.Add(hitRenderer);
        }

        foreach (Renderer occluder in nextOccluders) {
            if (!activeOccluders.Contains(occluder)) {
                SetRendererMode(occluder, ClipMode.Hole);
            }

            SetRendererHoleData(occluder, targetPosition);
        }

        foreach (Renderer previousOccluder in activeOccluders) {
            if (!nextOccluders.Contains(previousOccluder)) {
                SetRendererMode(previousOccluder, ClipMode.None);
            }
        }

        activeOccluders.Clear();
        foreach (Renderer occluder in nextOccluders) {
            activeOccluders.Add(occluder);
        }
    }

    private void SetRendererHoleData(Renderer targetRenderer, Vector3 holeCenter) {
        if (targetRenderer == null || cachedCamera == null) {
            return;
        }

        if (!rendererStates.TryGetValue(targetRenderer, out RendererState state) || state?.HoleSharedMaterials == null) {
            return;
        }

        Vector3 centerScreen = cachedCamera.WorldToScreenPoint(holeCenter);
        Vector3 centerViewport = cachedCamera.WorldToViewportPoint(holeCenter);
        Vector3 edgeScreen = cachedCamera.WorldToScreenPoint(holeCenter + cachedCamera.transform.right * holeRadius);
        Vector3 softnessEdgeScreen = cachedCamera.WorldToScreenPoint(holeCenter + cachedCamera.transform.right * (holeRadius + holeSoftness));

        float holeRadiusPixels = Mathf.Max(minimumHoleRadiusPixels, Vector2.Distance(centerScreen, edgeScreen));
        float holeSoftnessPixels = Mathf.Max(minimumHoleSoftnessPixels, Vector2.Distance(edgeScreen, softnessEdgeScreen));

        Material[] materials = state.HoleSharedMaterials;
        for (int i = 0; i < materials.Length; i++) {
            Material material = materials[i];
            if (material != null) {
                material.SetVector(HoleCenterViewportId, centerViewport);
                material.SetFloat(HoleRadiusPixelsId, holeRadiusPixels);
                material.SetFloat(HoleSoftnessPixelsId, holeSoftnessPixels);
            }
        }
    }

    private void ClearActiveOccluders() {
        foreach (Renderer previousOccluder in activeOccluders) {
            SetRendererMode(previousOccluder, ClipMode.None);
        }

        activeOccluders.Clear();
        nextOccluders.Clear();
    }

    private void SetRendererMode(Renderer targetRenderer, ClipMode mode) {
        if (targetRenderer == null || visibilityShader == null) {
            return;
        }

        if (!rendererStates.TryGetValue(targetRenderer, out RendererState state)) {
            state = new RendererState {
                OriginalSharedMaterials = targetRenderer.sharedMaterials,
                CurrentMode = ClipMode.None
            };

            rendererStates[targetRenderer] = state;
        }

        if (state.CurrentMode == mode) {
            return;
        }

        switch (mode) {
            case ClipMode.None:
                targetRenderer.sharedMaterials = state.OriginalSharedMaterials;
                break;
            case ClipMode.Hole:
                if (state.HoleSharedMaterials == null) {
                    state.HoleSharedMaterials = BuildReplacementMaterials(state.OriginalSharedMaterials);
                }

                targetRenderer.sharedMaterials = state.HoleSharedMaterials;
                break;
        }

        state.CurrentMode = mode;
    }

    private Material[] BuildReplacementMaterials(Material[] sourceMaterials) {
        if (sourceMaterials == null) {
            return null;
        }

        Material[] generated = new Material[sourceMaterials.Length];

        for (int i = 0; i < sourceMaterials.Length; i++) {
            Material source = sourceMaterials[i];
            Material generatedMaterial = new Material(visibilityShader) {
                name = source != null ? source.name + "_VisibilityClip" : "VisibilityClip"
            };

            if (source != null) {
                if (source.HasProperty(BaseMapId)) {
                    generatedMaterial.SetTexture(BaseMapId, source.GetTexture(BaseMapId));
                } else if (source.HasProperty("_MainTex")) {
                    generatedMaterial.SetTexture(BaseMapId, source.GetTexture("_MainTex"));
                }

                if (source.HasProperty(BaseColorId)) {
                    generatedMaterial.SetColor(BaseColorId, source.GetColor(BaseColorId));
                } else if (source.HasProperty("_Color")) {
                    generatedMaterial.SetColor(BaseColorId, source.GetColor("_Color"));
                }
            }

            generatedMaterial.SetFloat(HoleEnabledId, 1f);
            generatedMaterial.SetFloat(PlaneEnabledId, 0f);
            generatedMaterial.SetFloat(HoleRadiusPixelsId, 16f);
            generatedMaterial.SetFloat(HoleSoftnessPixelsId, 8f);
            generatedMaterial.SetVector(HoleCenterViewportId, Vector3.zero);

            generated[i] = generatedMaterial;
        }

        return generated;
    }

    private void RestoreAllRenderers() {
        foreach (KeyValuePair<Renderer, RendererState> pair in rendererStates) {
            Renderer renderer = pair.Key;
            RendererState state = pair.Value;

            if (renderer != null && state != null && state.OriginalSharedMaterials != null) {
                renderer.sharedMaterials = state.OriginalSharedMaterials;
            }

            if (state != null) {
                DestroyGeneratedMaterials(state.HoleSharedMaterials);
            }
        }

        rendererStates.Clear();
        activeOccluders.Clear();
        nextOccluders.Clear();
    }

    private void RefreshGeneratedMaterialSettings() {
        foreach (KeyValuePair<Renderer, RendererState> pair in rendererStates) {
            RendererState state = pair.Value;
            if (state == null) {
                continue;
            }

            ApplyHoleSettings(state.HoleSharedMaterials);
        }
    }

    private void ApplyHoleSettings(Material[] materials) {
        if (materials == null) {
            return;
        }

        for (int i = 0; i < materials.Length; i++) {
            Material material = materials[i];
            if (material == null) {
                continue;
            }

            // Radius and softness in pixels are recalculated each frame from holeRadius/holeSoftness.
        }
    }

    private void DestroyGeneratedMaterials(Material[] materials) {
        if (materials == null) {
            return;
        }

        for (int i = 0; i < materials.Length; i++) {
            Material material = materials[i];
            if (material != null) {
                Destroy(material);
            }
        }
    }
}
