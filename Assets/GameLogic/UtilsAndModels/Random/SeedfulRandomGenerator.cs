using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
    public class SeedfulRandomGenerator
    {
        private System.Random seedfullRandom;

        public SeedfulRandomGenerator(int seed)
        {
            seedfullRandom = new System.Random(seed);
            Debug.Log("Seed set to " + seedfullRandom.GetHashCode());
        }
        
        public int GetNumber()
        {
            return seedfullRandom.Next();
        }

        public int Range(int from, int to)
        {
            return seedfullRandom.Next(from, to);
        }

        public float Range(float from, float to)
        {
            var zeroToOneRandom = RangeZeroToOne();
            return GetAdjustedFloat(zeroToOneRandom, from, to);
        }

        public float RangeZeroToOne()
        {
            return (float)seedfullRandom.NextDouble();
        }

        public Vector2 GetInsideUnitCircle()
        {
            return new Vector2(
                Range(-1, (float)1),
                Range(-1, (float)1));
        }

        public void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var temp = list[i];
                var randomIndex = Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        public List<float> GetRangeZeroToOneList(int length)
        {
            var zeroToOneArray = new List<float>();

            for (int i = 0; i < length; i++)
            {
                zeroToOneArray.Add(RangeZeroToOne());
            }

            return zeroToOneArray;
        }

        public bool IsTrueIn50Precent()
        {
            return Range(0, 2) == 0;
        }

        private float GetAdjustedFloat(float zeroToOneValue, float from, float to)
        {
            return zeroToOneValue * (to - from) + from;
        }

        public Direction GetRandomDirection()
        {
            return (Direction)Range(0, Enum.GetValues(typeof(Direction)).Length);
        }
    }
}
