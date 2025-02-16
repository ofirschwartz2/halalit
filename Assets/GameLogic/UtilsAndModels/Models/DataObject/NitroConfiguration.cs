using UnityEngine;

namespace Assets.Models
{
    [CreateAssetMenu(fileName = "NitroFuelConfig", menuName = "Utilities/Nitro Configuration")]
    public class NitroConfiguration : BaseUtilityConfiguration
    {
        [SerializeField] private float speedMultiplier = 10f;
        [SerializeField] private float forceMultiplier = 2f;
        [SerializeField] private float maxGasAmount = 100f;
        [SerializeField] private float gasConsumptionMultiplier = 1f;

        public float SpeedMultiplier => speedMultiplier;
        public float ForceMultiplier => forceMultiplier;
        public float MaxGasAmount => maxGasAmount;
        public float GasConsumptionMultiplier => gasConsumptionMultiplier;
    }
} 