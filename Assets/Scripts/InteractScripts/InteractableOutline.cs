using UnityEngine;

public class InteractableOutline : MonoBehaviour {
    [Tooltip("O material que será usado como borda (Outline).")]
    [SerializeField] private Material outlineMaterial;
    [Tooltip("Renderers afetados pelo outline. Se vazio, usa todos os renderers filhos.")]
    [SerializeField] private Renderer[] targetRenderers;
    [Tooltip("Inclui renderers inativos ao buscar automaticamente.")]
    [SerializeField] private bool includeInactiveChildren = true;

    [Header("Outline Settings")]
    [SerializeField, Min(0f)] private float outlineThickness = 0.2f;
    [SerializeField, Min(0f)] private float outlineMinThickness = 0.1f;
    [SerializeField, Min(0f)] private float outlineSpeed = 1f;

    [Tooltip("O nome exato da propriedade de cor no Shader de Outline (Ex: _OutlineColor, _Color, _BorderColor)")]
    [SerializeField] private string colorPropertyName = "_OutlineColor";

    private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
    private static readonly int OutlineMinThicknessId = Shader.PropertyToID("_OutlineMinThickness");
    private static readonly int OutlineSpeedId = Shader.PropertyToID("_OutlineSpeed");

    private Renderer _renderer;
    private Material[] _originalMaterials;
    private Material[] _outlinedMaterials;
    private Material[][] _originalMaterialsByRenderer;
    private bool _isOutlined;

    // Guardamos a instância do material para modificá-la sem afetar o projeto original
    private Material _instancedOutlineMaterial;

    private void Awake() {
        _renderer = GetComponent<Renderer>();
        _originalMaterials = _renderer.materials; // Isso já cria instâncias dos materiais base

        _outlinedMaterials = new Material[_originalMaterials.Length + 1];
        _originalMaterials.CopyTo(_outlinedMaterials, 0);

        // Cria uma CÓPIA do material de outline específica para este objeto
        _instancedOutlineMaterial = new Material(outlineMaterial);
        _outlinedMaterials[_outlinedMaterials.Length - 1] = _instancedOutlineMaterial;
        ApplyOutlineSettingsToInstance();
        EnsureTargetRenderers();
    }

    private void OnValidate() {
        ApplyOutlineSettingsToInstance();
        EnsureTargetRenderers();
    }

    private void ApplyOutlineSettingsToInstance()
    {
        if (_instancedOutlineMaterial == null)
        {
            return;
        }

        if (_instancedOutlineMaterial.HasProperty(OutlineThicknessId))
        {
            _instancedOutlineMaterial.SetFloat(OutlineThicknessId, outlineThickness);
        }

        if (_instancedOutlineMaterial.HasProperty(OutlineMinThicknessId))
        {
            _instancedOutlineMaterial.SetFloat(OutlineMinThicknessId, outlineMinThickness);
        }

        if (_instancedOutlineMaterial.HasProperty(OutlineSpeedId))
        {
            _instancedOutlineMaterial.SetFloat(OutlineSpeedId, outlineSpeed);
        }
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
        if (_isOutlined || _instancedOutlineMaterial == null) {
            return;
        }

        ApplyOutlineSettingsToInstance();
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
                renderer.materials = new[] { _instancedOutlineMaterial };
                continue;
            }

            var outlinedMaterials = new Material[currentMaterials.Length + 1];
            currentMaterials.CopyTo(outlinedMaterials, 0);
            outlinedMaterials[outlinedMaterials.Length - 1] = _instancedOutlineMaterial;
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

    public void ChangeColor(Color color) {
        if (_instancedOutlineMaterial != null) {
            // Usa SetColor para mirar na propriedade correta do Shader
            if (_instancedOutlineMaterial.HasProperty(colorPropertyName)) {
                _instancedOutlineMaterial.SetColor(colorPropertyName, color);
            } else {
                // Fallback de segurança caso o shader realmente use a propriedade padrão
                _instancedOutlineMaterial.color = color;
            }

            if (_isOutlined && targetRenderers != null) {
                for (int i = 0; i < targetRenderers.Length; i++) {
                    var renderer = targetRenderers[i];
                    if (renderer == null) {
                        continue;
                    }

                    var currentMaterials = renderer.materials;
                    renderer.materials = currentMaterials;
                }
            }
        }
    }

    private void OnDestroy() {
        if (_instancedOutlineMaterial != null) {
            Destroy(_instancedOutlineMaterial);
        }
    }
}