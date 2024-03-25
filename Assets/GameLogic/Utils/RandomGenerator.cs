
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

            seedfullRandom = new System.Random(GetRandomInt());

            Debug.Log("Seed set to " + seedfullRandom.GetHashCode());
        }

        public static int GetRandomInt(bool fromSeed = false)
        {
            if (fromSeed)
            {
                return seedfullRandom.Next();
            }
            return seedlessRandom.Next();
        }

        public static int GetRandomInt(int from, int to, bool fromSeed = false)
        {
            if (fromSeed)
            {
                return seedfullRandom.Next(from, to);
            }
            return seedlessRandom.Next(from, to);
        }

        public static float GetRandomFloat(float from, float to, bool fromSeed = false)
        {
            var zeroToOneRandom = GetRandomFloatBetweenZeroToOne(fromSeed);
            return GetAdjustedFloat(zeroToOneRandom, from, to);
        }

        public static float GetRandomFloatBetweenZeroToOne(bool fromSeed = false)
        {
            if (fromSeed)
            {
                return (float)seedfullRandom.NextDouble();
            }
            return (float)seedlessRandom.NextDouble();
        }

        private static float GetAdjustedFloat(float zeroToOneValue, float from, float to)
        {
            return zeroToOneValue * (to - from) + from;
        }
    }
}
