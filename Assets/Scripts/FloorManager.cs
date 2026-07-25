using UnityEngine;

public class FloorManager : MonoBehaviour {
    public static FloorManager Instance { get; private set; }

    [Header("Configuração de Andares")]
    [Tooltip("Coloque os GameObjects pais de cada andar aqui, em ordem (0 = Térreo, 1 = 2º Andar, etc.)")]
    [SerializeField] private GameObject[] floors;

    public int CurrentFloorIndex { get; private set; } = 0;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowFloor(int floorIndex) {
        if (floorIndex < 0 || floorIndex >= floors.Length) return;

        CurrentFloorIndex = floorIndex;

        for (int i = 0; i < floors.Length; i++) {
            // Ativa apenas o andar atual e desativa o resto
            floors[i].SetActive(i <= floorIndex);
        }
    }
}