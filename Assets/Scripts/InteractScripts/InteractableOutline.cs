using UnityEngine;

public class InteractableOutline : MonoBehaviour {
    [Tooltip("O material que será usado como borda (Outline).")]
    [SerializeField] private Material outlineMaterial;

    private Renderer _renderer;
    private Material[] _originalMaterials;
    private bool _isOutlined;

    private void Awake() {
        _renderer = GetComponent<Renderer>();
        CacheCurrentMaterials();
    }

    public void DisableOutline() {
        if (_renderer != null && _originalMaterials != null && _isOutlined) {
            _renderer.materials = _originalMaterials;
            _isOutlined = false;
        }
    }

    public void EnableOutline() {
        if (_renderer == null || outlineMaterial == null) {
            return;
        }

        CacheCurrentMaterials();
        if (_originalMaterials == null) {
            return;
        }

        var outlinedMaterials = new Material[_originalMaterials.Length + 1];
        _originalMaterials.CopyTo(outlinedMaterials, 0);
        outlinedMaterials[outlinedMaterials.Length - 1] = outlineMaterial;
        _renderer.materials = outlinedMaterials;
        _isOutlined = true;
    }

    public void RefreshMaterials() {
        if (!_isOutlined) {
            CacheCurrentMaterials();
        }
    }

    private void CacheCurrentMaterials() {
        if (_renderer == null) {
            return;
        }

        _originalMaterials = _renderer.materials;
    }
}
