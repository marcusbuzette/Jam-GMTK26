using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class CharacterDefinition : ScriptableObject
{
    [SerializeField] private string characterName = "Character";
    [SerializeField] private GameObject portraitPrefab;
    [SerializeField] private Color characterAccentColor = Color.white;

    public string CharacterName => characterName;
    public GameObject PortraitPrefab => portraitPrefab;
    public Color CharacterAccentColor => characterAccentColor;
}
