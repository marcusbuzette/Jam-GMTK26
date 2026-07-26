#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CompleteLayerMaskAttribute))]
public class CompleteLayerMaskDrawer : PropertyDrawer
{
    private static readonly string[] LayerNames = BuildLayerNames();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.LayerMask)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        int newMask = EditorGUI.MaskField(position, label, property.intValue, LayerNames);
        property.intValue = newMask;
        EditorGUI.EndProperty();
    }

    private static string[] BuildLayerNames()
    {
        string[] names = new string[32];
        for (int index = 0; index < names.Length; index++)
        {
            string layerName = LayerMask.LayerToName(index);
            names[index] = string.IsNullOrEmpty(layerName) ? $"Layer {index}" : layerName;
        }

        return names;
    }
}
#endif