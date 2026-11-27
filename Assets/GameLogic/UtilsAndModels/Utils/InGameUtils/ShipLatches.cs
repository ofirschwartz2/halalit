using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
    // Leeches (#215) latched onto the Halalit: at most Constants.MAX_LATCHED_LEECHES, each slowing the ship's thrust by its factor.
    // Destroyed latchers are pruned, as dead decoys are in EnemyTarget.
    public static class ShipLatches
    {
        private static readonly List<LatchEntry> _latches = new();

        private struct LatchEntry
        {
            public Transform Latcher;
            public float ThrustFactor;
        }

        public static bool TryLatch(Transform latcher, float thrustFactor)
        {
            if (!HasFreeSlot())
            {
                return false;
            }

            _latches.Add(new LatchEntry { Latcher = latcher, ThrustFactor = thrustFactor });
            return true;
        }

        public static void Release(Transform latcher)
        {
            _latches.RemoveAll(latch => latch.Latcher == latcher);
        }

        public static bool HasFreeSlot()
        {
            PruneDestroyed();
            return _latches.Count < Constants.MAX_LATCHED_LEECHES;
        }

        // The product of the live latchers' factors; 1 when nothing is latched.
        public static float GetThrustFactor()
        {
            PruneDestroyed();

            float thrustFactor = 1f;
            foreach (LatchEntry latch in _latches)
            {
                thrustFactor *= latch.ThrustFactor;
            }
            return thrustFactor;
        }

        private static void PruneDestroyed()
        {
            for (int i = _latches.Count - 1; i >= 0; i--)
            {
                if (_latches[i].Latcher == null)
                {
                    _latches.RemoveAt(i);
                }
            }
        }

#if UNITY_EDITOR
        internal static int GetLatchedCount()
        {
            PruneDestroyed();
            return _latches.Count;
        }

        internal static void ClearAll()
        {
            _latches.Clear();
        }
#endif
    }
}
