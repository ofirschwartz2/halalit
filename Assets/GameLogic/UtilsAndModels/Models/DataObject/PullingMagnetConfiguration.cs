using UnityEngine;

namespace Assets.Models
{
    [CreateAssetMenu(fileName = "PullingMagnetConfig", menuName = "Utilities/Pulling Magnet Configuration")]
    public class PullingMagnetConfiguration : BaseUtilityConfiguration
    {
        [SerializeField] private float duration = 5f;
        [SerializeField] private float radius = 3f;
        [SerializeField] private float forceStrength = 10f;
        [SerializeField] private AnimationCurve forceCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public float Duration => duration;
        public float Radius => radius;
        public float ForceStrength => forceStrength;
        public AnimationCurve ForceCurve => forceCurve;
    }
} 