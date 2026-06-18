using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundBank))]
public class SoundBankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Auto Populate", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Missing Audio From Music/SFX Folders"))
        {
            PopulateSelectedSoundBanks(replaceExisting: false);
        }

        if (GUILayout.Button("Rebuild From Music/SFX Folders"))
        {
            bool rebuild = EditorUtility.DisplayDialog(
                "Rebuild Sound Bank",
                "This will clear the current Music and SFX lists, then rebuild them from Assets/music and Assets/Sfx.",
                "Rebuild",
                "Cancel");

            if (rebuild)
            {
                PopulateSelectedSoundBanks(replaceExisting: true);
            }
        }

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Scene Music", EditorStyles.boldLabel);

        if (GUILayout.Button("Sync Build Scene List"))
        {
            SyncSceneMusicFromBuildSettings(importSceneMusic: false);
        }

        if (GUILayout.Button("Import Music From Build Scenes"))
        {
            SyncSceneMusicFromBuildSettings(importSceneMusic: true);
        }
    }

    private void PopulateSelectedSoundBanks(bool replaceExisting)
    {
        foreach (Object selectedTarget in targets)
        {
            if (selectedTarget is not SoundBank soundBank) continue;

            Undo.RecordObject(soundBank, replaceExisting ? "Rebuild Sound Bank" : "Add Missing Audio To Sound Bank");
            soundBank.PopulateFromDefaultFolders(replaceExisting);
            EditorUtility.SetDirty(soundBank);
        }

        AssetDatabase.SaveAssets();
    }

    private void SyncSceneMusicFromBuildSettings(bool importSceneMusic)
    {
        foreach (Object selectedTarget in targets)
        {
            if (selectedTarget is not SoundBank soundBank) continue;

            Undo.RecordObject(
                soundBank,
                importSceneMusic ? "Import Scene Music To Sound Bank" : "Sync Sound Bank Scene Music");

            if (importSceneMusic)
            {
                soundBank.ImportSceneMusicFromBuildSettings();
            }
            else
            {
                soundBank.SyncSceneMusicFromBuildSettings();
            }

            EditorUtility.SetDirty(soundBank);
        }

        AssetDatabase.SaveAssets();
    }
}
