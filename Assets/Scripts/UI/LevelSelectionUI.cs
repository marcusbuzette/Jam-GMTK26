using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private Transform buttonsContainer; // Objeto com componente Grid Layout Group
    [SerializeField] private GameObject levelButtonPrefab; // Prefab de um botão de fase

    private void Start()
    {
        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        if (GameManager.Instance == null) return;

        // Limpa botões antigos se houver
        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        LevelSetupSO[] levels = GameManager.Instance.GetAllLevels();

        for (int i = 0; i < levels.Length; i++)
        {
            int levelIndex = i; // Copia local da variável para o callback da lambda
            GameObject btnObj = Instantiate(levelButtonPrefab, buttonsContainer);

            // Atualiza o texto do botão (ex: "Fase 1", "Fase 2"...)
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = (levelIndex + 1).ToString();
            }

            // Configura o evento do clique
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => GameManager.Instance.SelectAndStartLevel(levelIndex));
            }
        }
    }
}