using UnityEngine;

namespace Assets.Models
{
    [CreateAssetMenu(fileName = "MagnetConfig", menuName = "Utilities/Magnet Configuration")]
    public class MagnetConfiguration : BaseUtilityConfiguration
    {
        [SerializeField] private float duration = 10f;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float forceStrength = 10f;
        [SerializeField] private AnimationCurve forceCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public float Duration => duration;
        public float Radius => radius;
        public float ForceStrength => forceStrength;
        public AnimationCurve ForceCurve => forceCurve;
    }
} 