using UnityEngine;

public class InteractableOutline : MonoBehaviour {
    [Tooltip("O material que será usado como borda (Outline).")]
    [SerializeField] private Material outlineMaterial;

    [Tooltip("O nome exato da propriedade de cor no Shader de Outline (Ex: _OutlineColor, _Color, _BorderColor)")]
    [SerializeField] private string colorPropertyName = "_OutlineColor";

    private Renderer _renderer;
    private Material[] _originalMaterials;
    private Material[] _outlinedMaterials;

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
    }

    public void DisableOutline() {
        if (_renderer != null) {
            _renderer.materials = _originalMaterials;
        }
    }

    public void EnableOutline() {
        if (_renderer != null) {
            _renderer.materials = _outlinedMaterials;
        }
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

            // Reatribui a array ao renderer para garantir que a Unity desenhe a atualização
            _renderer.materials = _outlinedMaterials;
        }
    }

    private void OnDestroy() {
        if (_instancedOutlineMaterial != null) {
            Destroy(_instancedOutlineMaterial);
        }
    }
}