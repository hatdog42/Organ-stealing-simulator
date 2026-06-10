using System;

namespace MiniGames
{
    public enum MajorMiniGameType
    {
        Maze,
        DebugButtons
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
}
