
using UnityEngine;

namespace Assets.Utils
{
    public static class RandomGenerator
    {
        private static System.Random seedlessRandom = new System.Random();
        private static System.Random seedfullRandom = null;

        public static void SetSeed()
        {
            if (seedfullRandom != null)
            {
                throw new System.Exception("Seed is already set");
            }

            seedfullRandom = new System.Random(GetNumber());

            Debug.Log("Seed set to " + seedfullRandom.GetHashCode());
        }

        public static int GetNumber(bool fromSeed = false)
        {
            if (fromSeed)
            {
                TrySetSeed();
                return seedfullRandom.Next();
            }
            return seedlessRandom.Next();
        }

        public static int Range(int from, int to, bool fromSeed = false)
        {
            if (fromSeed)
            {
                TrySetSeed();
                return seedfullRandom.Next(from, to);
            }
            return seedlessRandom.Next(from, to);
        }

        public static float Range(float from, float to, bool fromSeed = false)
        {
            var zeroToOneRandom = RangeZeroToOne(fromSeed);
            return GetAdjustedFloat(zeroToOneRandom, from, to);
        }

        public static float RangeZeroToOne(bool fromSeed = false)
        {
            if (fromSeed)
            {
                TrySetSeed();
                return (float)seedfullRandom.NextDouble();
            }
            return (float)seedlessRandom.NextDouble();
        }

        public static Vector2 GetInsideUnitCircle(bool fromSeed = false) 
        {
            return new Vector2(
                Range(-1, (float)1, fromSeed),
                Range(-1, (float)1, fromSeed));
        }

        private static float GetAdjustedFloat(float zeroToOneValue, float from, float to)
        {
            return zeroToOneValue * (to - from) + from;
        }

        private static void TrySetSeed()
        {
            if (!IsSeedSet())
            {
                SetSeed();
            }
        }

        private static bool IsSeedSet()
        {
            return seedfullRandom != null;
        }
    }
}
