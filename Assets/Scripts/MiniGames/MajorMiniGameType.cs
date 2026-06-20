using System;
using UnityEngine;

namespace MiniGames
{
    public enum MajorMiniGameType
    {
        Maze = 0,
        DebugButtons = 1,
        Wordle = 2,
        Fishing = 3
    }

    [Serializable]
    public class MajorMiniGameOption
    {
        public MajorMiniGameType type;
        public string displayName;

        public MajorMiniGameOption()
        {
        }

        public MajorMiniGameOption(MajorMiniGameType type, string displayName)
        {
            this.type = type;
            this.displayName = displayName;
        }
    }

    public static class MajorMiniGameDebugSettings
    {
        private const string ForceDebugMiniGameKey = "MajorMiniGame.ForceDebugMiniGame";

        public static bool HasForceDebugMiniGamePreference => PlayerPrefs.HasKey(ForceDebugMiniGameKey);

        public static bool ForceDebugMiniGame
        {
            get => PlayerPrefs.GetInt(ForceDebugMiniGameKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ForceDebugMiniGameKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
