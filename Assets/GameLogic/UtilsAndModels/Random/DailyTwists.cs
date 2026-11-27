using Assets.Enums;
using System;

namespace Assets.Utils
{
    // Daily Twist (#239): the date picks one twist for the daily, the same on every phone.
    // Twists rotate (#273): the days are dealt in blocks of one day per twist, and each block gives every twist once,
    // in an order picked by DailySeed's hash of the block number. So one twist comes at most 2 days in a row.
    // The pick uses the date only and draws nothing from a seeded generator, so the rest of the run stays the same.
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

        public static DailyTwist PickForDate(DateTime israelDate)
        {
            // A day number is never below 0, so the division and the remainder are too
            int dayNumber = DailySeed.GetDayNumber(israelDate);
            int[] blockOrder = GetBlockOrder(dayNumber / PICKABLE.Length);

            return PICKABLE[blockOrder[dayNumber % PICKABLE.Length]];
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

        // A Fisher-Yates shuffle of the PICKABLE indexes. Each swap takes the next digit of the block number's hash, not a random draw
        private static int[] GetBlockOrder(int blockNumber)
        {
            var blockOrder = new int[PICKABLE.Length];
            for (var i = 0; i < blockOrder.Length; i++)
            {
                blockOrder[i] = i;
            }

            // A uint cast, so a negative hash still gives digits from 0 up
            uint hashBits = unchecked((uint)DailySeed.GetHash(blockNumber));

            for (var i = blockOrder.Length - 1; i > 0; i--)
            {
                uint choiceCount = (uint)(i + 1);
                int swapIndex = (int)(hashBits % choiceCount);
                hashBits /= choiceCount;

                int swappedIndex = blockOrder[i];
                blockOrder[i] = blockOrder[swapIndex];
                blockOrder[swapIndex] = swappedIndex;
            }

            return blockOrder;
        }
    }
}
