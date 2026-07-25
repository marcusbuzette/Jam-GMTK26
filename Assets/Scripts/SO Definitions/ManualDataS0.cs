using UnityEngine;

[CreateAssetMenu(fileName = "ManualDataSO", menuName = "Scriptable Objects/Manual Data")]
public class ManualDataSO : ScriptableObject
{
    [Header("Configuração das Páginas")]
    [Tooltip("Arraste os prefabs dos painéis de UI que representam cada página.")]
    public GameObject[] pagePrefabs;
}