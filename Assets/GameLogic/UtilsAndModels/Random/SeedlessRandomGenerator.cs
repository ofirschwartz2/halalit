using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
    public static class SeedlessRandomGenerator
    {
        private static System.Random seedlessRandom = new System.Random();

        public static int GetNumber()
        {
            return seedlessRandom.Next();
        }

        public static int Range(int from, int to)
        {
            return seedlessRandom.Next(from, to);
        }

        public static float Range(float from, float to)
        {
            var zeroToOneRandom = RangeZeroToOne();
            return GetAdjustedFloat(zeroToOneRandom, from, to);
        }

        public static float RangeZeroToOne()
        {
            return (float)seedlessRandom.NextDouble();
        }

        public static Vector2 GetInsideUnitCircle()
        {
            return new Vector2(
                Range(-1, (float)1),
                Range(-1, (float)1));
        }

        public static void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var randomIndex = Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public static List<float> GetRangeZeroToOneList(int length)
        {
            var zeroToOneArray = new List<float>();

            for (int i = 0; i < length; i++)
            {
                zeroToOneArray.Add(RangeZeroToOne());
            }

            return zeroToOneArray;
        }

        public static bool IsTrueIn50Precent()
        {
            return Range(0, 2) == 0;
        }

        private static float GetAdjustedFloat(float zeroToOneValue, float from, float to)
        {
            return zeroToOneValue * (to - from) + from;
        }
    }
}