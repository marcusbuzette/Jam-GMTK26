using UnityEngine;

public class InteractableOutline : MonoBehaviour {
    [Tooltip("O material que será usado como borda (Outline).")]
    [SerializeField] private Material outlineMaterial;

    private Renderer _renderer;
    private Material[] _originalMaterials;
    private Material[] _outlinedMaterials;

    private void Awake() {
        _renderer = GetComponent<Renderer>();
        _originalMaterials = _renderer.materials;

        _outlinedMaterials = new Material[_originalMaterials.Length + 1];

        _originalMaterials.CopyTo(_outlinedMaterials, 0);

        _outlinedMaterials[_outlinedMaterials.Length - 1] = outlineMaterial;
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
}
