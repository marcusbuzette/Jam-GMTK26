using UnityEngine;

public class InteractableOutline : MonoBehaviour {
    [Tooltip("O material que será usado como borda (Outline).")]
    [SerializeField] private Material outlineMaterial;
    [Tooltip("Renderers afetados pelo outline. Se vazio, usa todos os renderers filhos.")]
    [SerializeField] private Renderer[] targetRenderers;
    [Tooltip("Inclui renderers inativos ao buscar automaticamente.")]
    [SerializeField] private bool includeInactiveChildren = true;

    private Material[][] _originalMaterialsByRenderer;
    private bool _isOutlined;

    private void Awake() {
        EnsureTargetRenderers();
    }

    private void OnValidate()
    {
        EnsureTargetRenderers();
    }

    public void DisableOutline() {
        if (!_isOutlined || _originalMaterialsByRenderer == null || targetRenderers == null) {
            return;
        }

        int count = Mathf.Min(targetRenderers.Length, _originalMaterialsByRenderer.Length);
        for (int i = 0; i < count; i++) {
            if (targetRenderers[i] == null || _originalMaterialsByRenderer[i] == null) {
                continue;
            }

            targetRenderers[i].materials = _originalMaterialsByRenderer[i];
        }

        _isOutlined = false;
        _originalMaterialsByRenderer = null;
    }

    public void EnableOutline() {
        if (_isOutlined || outlineMaterial == null) {
            return;
        }

        EnsureTargetRenderers();
        if (targetRenderers == null || targetRenderers.Length == 0) {
            return;
        }

        _originalMaterialsByRenderer = new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++) {
            var renderer = targetRenderers[i];
            if (renderer == null) {
                continue;
            }

            var currentMaterials = renderer.materials;
            _originalMaterialsByRenderer[i] = currentMaterials;

            if (currentMaterials == null || currentMaterials.Length == 0) {
                renderer.materials = new[] { outlineMaterial };
                continue;
            }

            var outlinedMaterials = new Material[currentMaterials.Length + 1];
            currentMaterials.CopyTo(outlinedMaterials, 0);
            outlinedMaterials[outlinedMaterials.Length - 1] = outlineMaterial;
            renderer.materials = outlinedMaterials;
        }

        _isOutlined = true;
    }

    public void RefreshMaterials() {
        if (_isOutlined) {
            return;
        }

        _originalMaterialsByRenderer = null;
        EnsureTargetRenderers();
    }

    private void EnsureTargetRenderers() {
        if (targetRenderers != null && targetRenderers.Length > 0) {
            return;
        }

        targetRenderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);
    }
}
