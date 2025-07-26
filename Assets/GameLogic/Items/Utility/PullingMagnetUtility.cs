using Assets.Models;
using UnityEngine;

namespace Items.Utility
{
    public class PullingMagnetUtility : IUtility
    {
        private readonly PullingMagnetConfiguration _configuration;
        private GameObject _magnetInstance;
        private bool _isActive;
        private float _deactivationTime;
        private static readonly string MagnetPrefabPath = "Prefabs/Utilities/PullingMagnet";

        public bool IsActive => _isActive;

        public PullingMagnetUtility(PullingMagnetConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanActivate()
        {
            return !_isActive;
        }

        public void Activate(GameObject target)
        {
            if (!CanActivate()) return;

            Vector3 spawnPosition = target.transform.position;
            var prefab = Resources.Load<GameObject>(MagnetPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"PullingMagnetUtility: Could not find prefab at {MagnetPrefabPath}");
                return;
            }
            _magnetInstance = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);

            var field = _magnetInstance.GetComponent<PullingMagnetField>();
            if (field != null)
            {
                field.Initialize(_configuration);
            }
            else
            {
                Debug.LogError("PullingMagnetUtility: Magnet prefab missing PullingMagnetField component");
            }

            _isActive = true;
            _deactivationTime = Time.time + _configuration.Duration;
        }

        public void Deactivate()
        {
            if (!_isActive) return;
            if (_magnetInstance != null)
            {
                var field = _magnetInstance.GetComponent<PullingMagnetField>();
                if (field != null)
                {
                    field.FadeAndDestroy();
                }
                else
                {
                    Object.Destroy(_magnetInstance);
                }
            }
            _isActive = false;
        }

        public bool ShouldDeactivate()
        {
            if (!_isActive) return false;
            if (Time.time >= _deactivationTime)
            {
                return true;
            }
            return false;
        }
    }
} 