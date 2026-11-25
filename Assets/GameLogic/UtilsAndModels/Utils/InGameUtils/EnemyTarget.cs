using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
    // What enemies aim at: the newest live decoy, else the Halalit.
    public static class EnemyTarget
    {
        private static readonly List<Transform> _decoys = new();

        public static void AddDecoy(Transform decoy)
        {
            _decoys.Add(decoy);
        }

        public static void RemoveDecoy(Transform decoy)
        {
            _decoys.Remove(decoy);
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

        public static Vector2 GetDirection(Vector2 from)
        {
            var targetPosition = GetPosition();
            return new Vector2(targetPosition.x, targetPosition.y) - from;
        }
    }
}
