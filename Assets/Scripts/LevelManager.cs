using System;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public enum LevelState {
    Setup,
    Playing,
    Victory,
    Defeat
}

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance { get; private set; }

    [Header("Configuração Atual")]
    [SerializeField] private LevelSetupSO currentLevelData;
    [SerializeField] private Transform itemsContainer; // Container para instanciar os itens do cenário

    [Header("Referências Globais")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerPrefab; // Prefab do player, caso seja necessário instanciá-lo
    [SerializeField] private NavMeshSurface levelNavMesh; // Referência ao NavMesh do nível, caso seja necessário para navegação
    [SerializeField] private CinemachineCamera cmCam; // Referência à câmera principal, caso seja necessário para ajustes de zoom ou posicionamento

    // Estado Runtime
    public LevelState CurrentState { get; private set; } = LevelState.Setup;
    public float RemainingTime { get; private set; }

    // Eventos para acoplar UI e Audio sem dependência direta
    public static event Action<float> OnTimerUpdated; // Passa o tempo restante
    public static event Action OnLevelStarted;
    public static event Action OnLevelVictory;
    public static event Action OnLevelDefeat;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        // Pega o SO selecionado no GameManager
        if (GameManager.Instance != null && GameManager.Instance.SelectedLevel != null) {
            LoadLevel(GameManager.Instance.SelectedLevel);
        } else if (currentLevelData != null) {
            LoadLevel(currentLevelData);
        } else {
            Debug.LogError("Nenhum LevelDataSO foi fornecido ao LevelManager!");
        }
    }

    private void Update() {
        if (CurrentState != LevelState.Playing) return;
        UpdateTimer();
    }


    public void LoadLevel(LevelSetupSO levelData) {
        currentLevelData = levelData;
        CurrentState = LevelState.Setup;

        // Configurar Timer
        RemainingTime = currentLevelData.durationInSeconds;

        // Posicionar Player
        if (playerTransform != null) {
            playerTransform.GetComponent<CharacterController>().enabled = false; // Desativar temporariamente o CharacterController para evitar colisões durante o posicionamento
            playerTransform.GetComponent<NavMeshAgent>().enabled = false; // Desativar temporariamente o NavMeshAgent para evitar colisões durante o posicionamento
            // Se estiver usando NavMeshAgent no player, desative-o antes do Warp ou Mova pelo agent.Warp
            playerTransform.position = currentLevelData.playerSpawnPosition;
            playerTransform.rotation = Quaternion.Euler(currentLevelData.playerSpawnRotation);
            playerTransform.GetComponent<CharacterController>().enabled = true;
            playerTransform.GetComponent<NavMeshAgent>().enabled = true;
        } else {
            // Se o player não estiver presente na cena, instancia
            GameObject playerInstance = Instantiate(playerPrefab, currentLevelData.playerSpawnPosition,
                                            Quaternion.Euler(currentLevelData.playerSpawnRotation));
            playerTransform = playerInstance.transform;
        }

        cmCam.Follow = playerTransform;


        //Spawnar Prefab dos itens de Cenário em suas posicoes configuradas
        SpawnLevelPrefab();

        //Iniciar Gameplay
        CurrentState = LevelState.Playing;
        OnLevelStarted?.Invoke();
    }

    private void SpawnLevelPrefab() {
        if (currentLevelData.itemsToSpawn != null && itemsContainer != null) {
            foreach (Transform child in itemsContainer) {
                Destroy(child.gameObject);
            }

            foreach (Transform child in currentLevelData.itemsToSpawn.transform) {
                GameObject itemInstance = Instantiate(child.gameObject, itemsContainer);
                itemInstance.transform.localPosition = child.localPosition;
                itemInstance.transform.localRotation = child.localRotation;
            }

            playerTransform.gameObject.SetActive(false); // Desativar temporariamente o player para evitar colisões durante a reconstrução do NavMesh

            levelNavMesh.BuildNavMesh(); // Reconstruir NavMesh após instanciar os itens do cenário

            playerTransform.gameObject.SetActive(true);
        }
    }

    private void UpdateTimer() {
        RemainingTime -= Time.deltaTime;
        OnTimerUpdated?.Invoke(RemainingTime);

        if (RemainingTime <= 0f) {
            RemainingTime = 0f;
            TriggerDefeat("O tempo acabou! A bomba explodiu.");
        }
    }


    public void TriggerVictory() {
        if (CurrentState != LevelState.Playing) return;

        CurrentState = LevelState.Victory;
        OnLevelVictory?.Invoke();
        
        GameManager.Instance.LoadNextLevel();
    }


    public void TriggerDefeat(string reason) {
        if (CurrentState != LevelState.Playing) return;

        CurrentState = LevelState.Defeat;
        OnLevelDefeat?.Invoke();
        Debug.Log($"DERROTA: {reason}");
    }
}