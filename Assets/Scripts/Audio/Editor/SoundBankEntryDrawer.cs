using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SoundBankEntry))]
public class SoundBankEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, EntryLabel(property, label), includeChildren: true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, includeChildren: true);
    }

    private static GUIContent EntryLabel(SerializedProperty property, GUIContent fallbackLabel)
    {
        SerializedProperty idProperty = property.FindPropertyRelative("id");
        if (idProperty != null && idProperty.enumValueIndex > 0)
        {
            return new GUIContent(idProperty.enumDisplayNames[idProperty.enumValueIndex]);
        }

        SerializedProperty clipProperty = property.FindPropertyRelative("clip");
        if (clipProperty?.objectReferenceValue)
        {
            return new GUIContent(clipProperty.objectReferenceValue.name);
        }

        return fallbackLabel;
    }
}
