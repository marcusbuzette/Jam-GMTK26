using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopDownVisibilitySystem : MonoBehaviour
{
    private enum ClipMode
    {
        None,
        Hole,
        Plane
    }

    private sealed class RendererState
    {
        public Material[] OriginalSharedMaterials;
        public Material[] HoleSharedMaterials;
        public Material[] PlaneSharedMaterials;
        public ClipMode CurrentMode;
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private Shader visibilityShader;

    [Header("Occluder Hole")]
    [SerializeField] private LayerMask obstructionMask = ~0;
    [SerializeField, Min(0.01f)] private float castRadius = 0.35f;
    [SerializeField, Min(0.01f)] private float holeRadius = 1.45f;
    [SerializeField, Min(0f)] private float holeSoftness = 0.35f;

    [Header("Map Front Clipping")]
    [SerializeField] private bool hideMapInFrontOfCamera = true;
    [SerializeField, Min(0f)] private float frontClipOffsetFromTarget = 0f;

    [Header("Target Offset")]
    [SerializeField] private Vector3 targetWorldOffset = new Vector3(0f, 0.9f, 0f);

    private Camera cachedCamera;
    private readonly Dictionary<Renderer, RendererState> rendererStates = new Dictionary<Renderer, RendererState>();
    private readonly HashSet<Renderer> activeOccluders = new HashSet<Renderer>();
    private readonly HashSet<Renderer> nextOccluders = new HashSet<Renderer>();
    private readonly HashSet<Renderer> mapRenderers = new HashSet<Renderer>();

    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int HoleEnabledId = Shader.PropertyToID("_HoleEnabled");
    private static readonly int PlaneEnabledId = Shader.PropertyToID("_PlaneEnabled");
    private static readonly int CutoutRadiusId = Shader.PropertyToID("_CutoutRadius");
    private static readonly int CutoutSoftnessId = Shader.PropertyToID("_CutoutSoftness");
    private static readonly int GlobalTargetPosId = Shader.PropertyToID("_Vis_TargetPos");
    private static readonly int GlobalPlanePointId = Shader.PropertyToID("_Vis_PlanePoint");
    private static readonly int GlobalPlaneNormalId = Shader.PropertyToID("_Vis_PlaneNormal");

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();

        if (visibilityShader == null)
        {
            visibilityShader = Shader.Find("Custom/URPVisibilityClip");
        }
    }

    private void OnEnable()
    {
        BuildMapRendererSet();
        ApplyMapClipMode();
    }

    private void OnDisable()
    {
        RestoreAllRenderers();
    }

    private void LateUpdate()
    {
        if (target == null || visibilityShader == null)
        {
            return;
        }

        RefreshGeneratedMaterialSettings();

        Vector3 targetPosition = target.position + targetWorldOffset;
        Vector3 cameraPosition = cachedCamera.transform.position;
        Vector3 cameraToTarget = targetPosition - cameraPosition;
        float distanceToTarget = cameraToTarget.magnitude;

        if (distanceToTarget <= Mathf.Epsilon)
        {
            return;
        }

        Vector3 directionToTarget = cameraToTarget / distanceToTarget;

        Shader.SetGlobalVector(GlobalTargetPosId, targetPosition);

        if (hideMapInFrontOfCamera)
        {
            Vector3 planePoint = targetPosition + directionToTarget * frontClipOffsetFromTarget;
            Shader.SetGlobalVector(GlobalPlanePointId, planePoint);
            Shader.SetGlobalVector(GlobalPlaneNormalId, directionToTarget);
        }

        UpdateOccluders(cameraPosition, directionToTarget, distanceToTarget);
    }

    private void UpdateOccluders(Vector3 cameraPosition, Vector3 directionToTarget, float distanceToTarget)
    {
        nextOccluders.Clear();

        RaycastHit[] hits = Physics.SphereCastAll(
            cameraPosition,
            castRadius,
            directionToTarget,
            distanceToTarget,
            obstructionMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
            {
                continue;
            }

            if (target != null && hitCollider.transform.IsChildOf(target))
            {
                continue;
            }

            Renderer hitRenderer = hitCollider.GetComponentInParent<Renderer>();
            if (hitRenderer == null || mapRenderers.Contains(hitRenderer))
            {
                continue;
            }

            nextOccluders.Add(hitRenderer);
        }

        foreach (Renderer occluder in nextOccluders)
        {
            if (!activeOccluders.Contains(occluder))
            {
                SetRendererMode(occluder, ClipMode.Hole);
            }
        }

        foreach (Renderer previousOccluder in activeOccluders)
        {
            if (!nextOccluders.Contains(previousOccluder))
            {
                SetRendererMode(previousOccluder, ClipMode.None);
            }
        }

        activeOccluders.Clear();
        foreach (Renderer occluder in nextOccluders)
        {
            activeOccluders.Add(occluder);
        }
    }

    private void BuildMapRendererSet()
    {
        mapRenderers.Clear();

        if (mapRoot == null)
        {
            return;
        }

        Renderer[] foundRenderers = mapRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < foundRenderers.Length; i++)
        {
            Renderer renderer = foundRenderers[i];
            if (renderer != null)
            {
                mapRenderers.Add(renderer);
            }
        }
    }

    private void ApplyMapClipMode()
    {
        foreach (Renderer mapRenderer in mapRenderers)
        {
            SetRendererMode(mapRenderer, hideMapInFrontOfCamera ? ClipMode.Plane : ClipMode.None);
        }
    }

    private void SetRendererMode(Renderer targetRenderer, ClipMode mode)
    {
        if (targetRenderer == null || visibilityShader == null)
        {
            return;
        }

        if (!rendererStates.TryGetValue(targetRenderer, out RendererState state))
        {
            state = new RendererState
            {
                OriginalSharedMaterials = targetRenderer.sharedMaterials,
                CurrentMode = ClipMode.None
            };

            rendererStates[targetRenderer] = state;
        }

        if (state.CurrentMode == mode)
        {
            return;
        }

        switch (mode)
        {
            case ClipMode.None:
                targetRenderer.sharedMaterials = state.OriginalSharedMaterials;
                break;
            case ClipMode.Hole:
                if (state.HoleSharedMaterials == null)
                {
                    state.HoleSharedMaterials = BuildReplacementMaterials(state.OriginalSharedMaterials, true, false);
                }

                targetRenderer.sharedMaterials = state.HoleSharedMaterials;
                break;
            case ClipMode.Plane:
                if (state.PlaneSharedMaterials == null)
                {
                    state.PlaneSharedMaterials = BuildReplacementMaterials(state.OriginalSharedMaterials, false, true);
                }

                targetRenderer.sharedMaterials = state.PlaneSharedMaterials;
                break;
        }

        state.CurrentMode = mode;
    }

    private Material[] BuildReplacementMaterials(Material[] sourceMaterials, bool enableHole, bool enablePlane)
    {
        if (sourceMaterials == null)
        {
            return null;
        }

        Material[] generated = new Material[sourceMaterials.Length];

        for (int i = 0; i < sourceMaterials.Length; i++)
        {
            Material source = sourceMaterials[i];
            Material generatedMaterial = new Material(visibilityShader)
            {
                name = source != null ? source.name + "_VisibilityClip" : "VisibilityClip"
            };

            if (source != null)
            {
                if (source.HasProperty(BaseMapId))
                {
                    generatedMaterial.SetTexture(BaseMapId, source.GetTexture(BaseMapId));
                }
                else if (source.HasProperty("_MainTex"))
                {
                    generatedMaterial.SetTexture(BaseMapId, source.GetTexture("_MainTex"));
                }

                if (source.HasProperty(BaseColorId))
                {
                    generatedMaterial.SetColor(BaseColorId, source.GetColor(BaseColorId));
                }
                else if (source.HasProperty("_Color"))
                {
                    generatedMaterial.SetColor(BaseColorId, source.GetColor("_Color"));
                }
            }

            generatedMaterial.SetFloat(HoleEnabledId, enableHole ? 1f : 0f);
            generatedMaterial.SetFloat(PlaneEnabledId, enablePlane ? 1f : 0f);
            generatedMaterial.SetFloat(CutoutRadiusId, holeRadius);
            generatedMaterial.SetFloat(CutoutSoftnessId, holeSoftness);

            generated[i] = generatedMaterial;
        }

        return generated;
    }

    private void RestoreAllRenderers()
    {
        foreach (KeyValuePair<Renderer, RendererState> pair in rendererStates)
        {
            Renderer renderer = pair.Key;
            RendererState state = pair.Value;

            if (renderer != null && state != null && state.OriginalSharedMaterials != null)
            {
                renderer.sharedMaterials = state.OriginalSharedMaterials;
            }

            if (state != null)
            {
                DestroyGeneratedMaterials(state.HoleSharedMaterials);
                DestroyGeneratedMaterials(state.PlaneSharedMaterials);
            }
        }

        rendererStates.Clear();
        activeOccluders.Clear();
        nextOccluders.Clear();
    }

    private void RefreshGeneratedMaterialSettings()
    {
        foreach (KeyValuePair<Renderer, RendererState> pair in rendererStates)
        {
            RendererState state = pair.Value;
            if (state == null)
            {
                continue;
            }

            ApplyHoleSettings(state.HoleSharedMaterials);
            ApplyHoleSettings(state.PlaneSharedMaterials);
        }
    }

    private void ApplyHoleSettings(Material[] materials)
    {
        if (materials == null)
        {
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material == null)
            {
                continue;
            }

            material.SetFloat(CutoutRadiusId, holeRadius);
            material.SetFloat(CutoutSoftnessId, holeSoftness);
        }
    }

    private void DestroyGeneratedMaterials(Material[] materials)
    {
        if (materials == null)
        {
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
