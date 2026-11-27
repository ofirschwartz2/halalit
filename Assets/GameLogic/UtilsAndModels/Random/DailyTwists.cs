using Assets.Enums;
using System;

namespace Assets.Utils
{
    // Daily Twist (#239): the date picks one twist for the daily, the same on every phone.
    // The pick uses the daily seed's value only and draws nothing from a seeded generator, so the rest of the run stays the same.
    // Add rows to PICKABLE at the end only, and ship a changed list in a build everyone installs:
    // a changed list deals a different twist for every date, so phones on an older build would play another twist.
    public static class DailyTwists
    {
        private static readonly DailyTwist NONE_TWIST = new DailyTwist(DailyTwistName.NONE, "", "", 0, 1f, 0f);

        private static readonly DailyTwist[] PICKABLE =
        {
            new DailyTwist(DailyTwistName.GLASS_DAY, "Glass Day", "Your ship has 12 health", 12, 1f, 0f),
            new DailyTwist(DailyTwistName.SWARM, "Swarm", "Enemies arrive faster", 0, 0.6f, 0f),
            new DailyTwist(DailyTwistName.BIG_ROCKS, "Big Rocks", "Only big asteroids", 0, 1f, 3f)
        };

        // Set by OriginalRandomSeed.Awake on every Playground load, so read it in Start
        public static DailyTwist Current { get; private set; } = NONE_TWIST;

        public static DailyTwist PickForSeed(int dailySeed)
        {
            // A uint cast, not Math.Abs: Math.Abs(int.MinValue) throws
            uint seedBits = unchecked((uint)dailySeed);
            return PICKABLE[(int)(seedBits % (uint)PICKABLE.Length)];
        }

        public static DailyTwist PickForDate(DateTime israelDate)
        {
            return PickForSeed(DailySeed.GetSeed(israelDate));
        }

        public static DailyTwist Get(DailyTwistName name)
        {
            foreach (var twist in PICKABLE)
            {
                if (twist.Name == name)
                {
                    return twist;
                }
            }

            return NONE_TWIST;
        }

        public static void SetCurrent(DailyTwist twist)
        {
            Current = twist;
        }
    }
}
