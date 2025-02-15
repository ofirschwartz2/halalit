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
        private float _endTime;

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
                Debug.LogError("NitroUtility: Failed to find ISpeedForceController component");
                return;
            }

            _originalSpeedLimit = _speedForceController.GetSpeedLimit();
            _originalForceMultiplier = _speedForceController.GetForceMultiplier();

            _speedForceController.SetSpeedLimit(_originalSpeedLimit * _configuration.SpeedMultiplier);
            _speedForceController.SetForceMultiplier(_originalForceMultiplier * _configuration.ForceMultiplier);

            _endTime = Time.time + _configuration.Duration;
            _configuration.Activate();

            Debug.Log($"NitroUtility: Activated. Speed limit increased to {_originalSpeedLimit * _configuration.SpeedMultiplier}, Force increased to {_originalForceMultiplier * _configuration.ForceMultiplier}");
        }

        public void Deactivate()
        {
            if (!IsActive || _speedForceController == null) return;

            _speedForceController.SetSpeedLimit(_originalSpeedLimit);
            _speedForceController.SetForceMultiplier(_originalForceMultiplier);
            _configuration.Reset();

            Debug.Log("NitroUtility: Deactivated. Speed and force reset to original values");
        }

        public bool ShouldDeactivate()
        {
            return IsActive && Time.time >= _endTime;
        }
    }
} 