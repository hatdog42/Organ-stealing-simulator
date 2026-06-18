using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneMusicEntry))]
public class SceneMusicEntryDrawer : PropertyDrawer
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
        SerializedProperty sceneName = property.FindPropertyRelative("sceneName");
        if (sceneName != null && !string.IsNullOrWhiteSpace(sceneName.stringValue))
        {
            return new GUIContent(sceneName.stringValue);
        }

        SerializedProperty scenePath = property.FindPropertyRelative("scenePath");
        if (scenePath != null && !string.IsNullOrWhiteSpace(scenePath.stringValue))
        {
            return new GUIContent(System.IO.Path.GetFileNameWithoutExtension(scenePath.stringValue));
        }

        return fallbackLabel;
    }
}
