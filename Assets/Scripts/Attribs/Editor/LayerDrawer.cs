using UnityEditor;
using UnityEngine;

// https://forum.unity.com/threads/how-to-get-name-or-number-of-selected-layer.445296/

[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, "The property has to be a layer for LayerAttribute to work!");
            return;
        }

        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}