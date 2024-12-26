using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum SceneName
    {
        [Description("Playground")]
        PLAYGROUND,
        [Description("MainMenu")]
        MAIN_MENU,
        [Description("Leaderboard")]
        LEADERBOARD,
    }
}
