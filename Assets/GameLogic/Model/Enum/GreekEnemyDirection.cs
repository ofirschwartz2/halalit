using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum VerticalGreekEnemyDirection
    {
        UP1 = 0,
        RIGHT = 1,
        UP2 = 2,
        LEFT = 3
    }

    [Serializable]
    public enum HorizontalGreekEnemyDirection
    {
        RIGHT1 = 0,
        UP = 1,
        RIGHT2 = 2,
        DOWN = 3
    }
}