using Assets.Models;
using UnityEngine;

namespace Items.Utility
{
    public class NitroUtility : IUtility
    {
        private readonly NitroConfiguration _configuration;
        private ISpeedForceController _speedForceController;
        private float _originalSpeedLimit;
        private float _originalForceMultiplier;
        private float _remainingGas;

        public bool IsActive => _configuration.IsActive;

        public NitroUtility(NitroConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanActivate()
        {
            return !IsActive;
        }

        public void Activate(GameObject target)
        {
            if (!CanActivate()) return;

            _speedForceController = target.GetComponent<ISpeedForceController>();
            
            if (_speedForceController == null)
            {
                Debug.LogError("NitroUtility: Failed to find required components");
                return;
            }

            _originalSpeedLimit = _speedForceController.GetSpeedLimit();
            _originalForceMultiplier = _speedForceController.GetForceMultiplier();

            _speedForceController.SetSpeedLimit(_originalSpeedLimit * _configuration.SpeedMultiplier);
            _speedForceController.SetForceMultiplier(_originalForceMultiplier * _configuration.ForceMultiplier);

            _remainingGas = _configuration.MaxGasAmount;
            _configuration.Activate();

            Debug.Log($"NitroUtility: Activated with {_remainingGas} gas. Speed limit increased to {_originalSpeedLimit * _configuration.SpeedMultiplier}, Force increased to {_originalForceMultiplier * _configuration.ForceMultiplier}");
        }

        public void Deactivate()
        {
            if (!IsActive || _speedForceController == null) return;

            _speedForceController.SetSpeedLimit(_originalSpeedLimit);
            _speedForceController.SetForceMultiplier(_originalForceMultiplier);
            
            _configuration.Reset();

            Debug.Log($"NitroUtility: Deactivated with {_remainingGas:F2} gas remaining. Speed and force reset to original values");
        }

        public bool ShouldDeactivate()
        {
            if (!IsActive) return false;

            // Calculate gas consumption based on the force being applied
            float forceAmount = _speedForceController.GetCurrentForce().magnitude;
            float gasConsumption = forceAmount * _configuration.GasConsumptionMultiplier * Time.deltaTime;
            _remainingGas -= gasConsumption;

            if (_remainingGas <= 0)
            {
                Debug.Log("NitroUtility: Out of gas!");
                return true;
            }

            if (forceAmount > 0)
            {
                Debug.Log($"NitroUtility: Force: {forceAmount:F2}, Gas remaining: {_remainingGas:F2}");
            }

            return false;
        }
    }
} 