using UnityEngine;

namespace Assets.Models
{
    [CreateAssetMenu(fileName = "NitroFuelConfig", menuName = "Utilities/Nitro Configuration")]
    public class NitroConfiguration : BaseUtilityConfiguration
    {
        [SerializeField] private float speedMultiplier = 10f;
        [SerializeField] private float forceMultiplier = 2f;

        public float SpeedMultiplier => speedMultiplier;
        public float ForceMultiplier => forceMultiplier;
    }
} 