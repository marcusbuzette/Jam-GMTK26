using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSetupSO", menuName = "Scriptable Objects/LevelSetupSO")]
public class LevelSetupSO : ScriptableObject {
    [Header("Identificação do Nível")]
    public string levelName = "Level 1";
    public int rpm = 63;
    public AudioClip levelMusic;

    [Header("Player Spawn Configuration")]
    public Vector3 playerSpawnPosition;
    public Vector3 playerSpawnRotation;
    public int startingFloorIndex = 0;


    [Header("Prefab com os itens a serem instanciados")]
    public GameObject itemsToSpawn;
}

[Serializable]
public struct NPCSpawnData {
    public GameObject npcPrefab;
    public Vector3 spawnPosition;
    public Quaternion rotation;
}

[Serializable]
public struct ItemSpawnData {
    public GameObject itemPrefab;
    public Vector3 spawnPosition;
}
