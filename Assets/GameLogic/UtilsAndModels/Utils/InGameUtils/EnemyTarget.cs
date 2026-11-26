using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

namespace Assets.Utils
{
    // What enemies aim at: the newest live decoy, else the Halalit.
    // Grudge (#216): an enemy that holds a live grudge aims at the enemy that hurt it instead.
    public static class EnemyTarget
    {
        private static readonly List<Transform> _decoys = new();
        private static readonly Dictionary<Transform, GrudgeEntry> _grudges = new();

        private struct GrudgeEntry
        {
            public Transform Target;
            public float EndTime;
        }

        public static void AddDecoy(Transform decoy)
        {
            _decoys.Add(decoy);
        }

        public static void RemoveDecoy(Transform decoy)
        {
            _decoys.Remove(decoy);
        }

        // A new grudge replaces the holder's old one.
        public static void SetGrudge(Transform holder, Transform target, float endTime)
        {
            _grudges[holder] = new GrudgeEntry { Target = target, EndTime = endTime };
        }

        public static void ClearGrudge(Transform holder)
        {
            _grudges.Remove(holder);
        }

        public static bool HasGrudge(Transform holder)
        {
            return TryGetGrudgeTarget(holder, out _);
        }

        public static Vector3 GetPosition()
        {
            while (_decoys.Count > 0 && _decoys[_decoys.Count - 1] == null)
            {
                _decoys.RemoveAt(_decoys.Count - 1);
            }

            if (_decoys.Count > 0)
            {
                return _decoys[_decoys.Count - 1].position;
            }

            return Utils.GetHalalitPosition();
        }

        public static Vector3 GetPosition(Transform asker)
        {
            if (TryGetGrudgeTarget(asker, out var grudgeTarget))
            {
                return grudgeTarget.position;
            }

            return GetPosition();
        }

        public static Vector2 GetDirection(Vector2 from)
        {
            var targetPosition = GetPosition();
            return new Vector2(targetPosition.x, targetPosition.y) - from;
        }

        public static Vector2 GetDirection(Transform asker)
        {
            var targetPosition = GetPosition(asker);
            return new Vector2(targetPosition.x - asker.position.x, targetPosition.y - asker.position.y);
        }

        // A grudge whose target is gone or whose time is over is removed here, as dead decoys are in GetPosition.
        private static bool TryGetGrudgeTarget(Transform holder, out Transform target)
        {
            target = null;
            if (!_grudges.TryGetValue(holder, out var grudge))
            {
                return false;
            }

            if (grudge.Target == null || Utils.ShouldDie(grudge.EndTime))
            {
                _grudges.Remove(holder);
                return false;
            }

            target = grudge.Target;
            return true;
        }

#if UNITY_EDITOR
        internal static void ClearAllGrudges()
        {
            _grudges.Clear();
        }
#endif
    }
}
