using System.IO;
using UnityEditor;
using UnityEngine;

public static class SoundBankAssetCreator
{
    private const string DefaultResourcesPath = "Assets/Resources";
    private const string DefaultAssetPath = DefaultResourcesPath + "/DefaultSoundBank.asset";

    [MenuItem("Assets/Create/Scriptable Objects/Audio/Sound Bank", priority = 120)]
    public static void CreateSoundBank()
    {
        string targetFolder = GetSelectedFolder();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(targetFolder, "SoundBank.asset"));
        CreateAssetAtPath(assetPath);
    }

    [MenuItem("Tools/Audio/Create Default Sound Bank")]
    public static void CreateDefaultSoundBank()
    {
        if (!AssetDatabase.IsValidFolder(DefaultResourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (AssetDatabase.LoadAssetAtPath<SoundBank>(DefaultAssetPath))
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SoundBank>(DefaultAssetPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            return;
        }

        CreateAssetAtPath(DefaultAssetPath);
    }

    private static void CreateAssetAtPath(string assetPath)
    {
        SoundBank soundBank = ScriptableObject.CreateInstance<SoundBank>();
        AssetDatabase.CreateAsset(soundBank, assetPath);
        soundBank.PopulateFromDefaultFolders(replaceExisting: true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = soundBank;
        EditorGUIUtility.PingObject(soundBank);
    }

    private static string GetSelectedFolder()
    {
        Object selected = Selection.activeObject;
        if (!selected) return "Assets";

        string path = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrEmpty(path)) return "Assets";

        return AssetDatabase.IsValidFolder(path) ? path : Path.GetDirectoryName(path);
    }
}
