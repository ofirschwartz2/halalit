using Assets.Enums;
using Assets.Utils;
using UnityEngine;

namespace Assets.Player
{
    public class UtilityManager : MonoBehaviour
    {
        private const float SPEED_MULTIPLIER = 10f;
        private const float FORCE_MULTIPLIER = 2f;
        private const float DURATION = 10f;

        private bool _hasNitroFuel;
        private bool _isNitroActive;
        private float _nitroEndTime;
        private GameObject _halalit;
        private HalalitMovement _halalitMovement;
        private float _originalSpeedLimit;
        private float _originalForceMultiplier;

        private void Awake()
        {
            Debug.Log("UtilityManager: Awake");
            SetEventListeners();
            TryInitializeHalalitReferences();
        }

        private bool TryInitializeHalalitReferences()
        {
            if (_halalitMovement != null) return true;

            Debug.Log("UtilityManager: Trying to initialize Halalit references");
            _halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
            if (_halalit == null)
            {
                Debug.LogWarning("UtilityManager: Could not find Halalit GameObject");
                return false;
            }

            _halalitMovement = _halalit.GetComponent<HalalitMovement>();
            if (_halalitMovement == null)
            {
                Debug.LogWarning("UtilityManager: Could not find HalalitMovement component on Halalit");
                return false;
            }

            _originalSpeedLimit = _halalitMovement.GetSpeedLimit();
            _originalForceMultiplier = _halalitMovement.GetForceMultiplier();
            Debug.Log($"UtilityManager: Successfully initialized. Original speed limit: {_originalSpeedLimit}, Original force: {_originalForceMultiplier}");
            return true;
        }

        private void SetEventListeners()
        {
            Debug.Log("UtilityManager: Setting up event listeners");
            ItemEvent.PlayerUtilityPickUp += OnUtilityPickup;
        }

        private void OnDestroy()
        {
            DestroyEventListeners();
        }

        private void DestroyEventListeners()
        {
            ItemEvent.PlayerUtilityPickUp -= OnUtilityPickup;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("UtilityManager: U key pressed");
                if (_hasNitroFuel && !_isNitroActive)
                {
                    ActivateNitro();
                }
                else
                {
                    Debug.Log($"UtilityManager: No utility to activate. Has Nitro: {_hasNitroFuel}, Is Active: {_isNitroActive}");
                }
            }

            if (_isNitroActive && Time.time >= _nitroEndTime)
            {
                DeactivateNitro();
            }
        }

        private void OnUtilityPickup(object initiator, ItemEventArguments arguments)
        {
            Debug.Log($"UtilityManager: Received pickup event for {arguments.Name}");
            if (arguments.Name == ItemName.NITRO_FUEL)
            {
                _hasNitroFuel = true;
                Debug.Log("UtilityManager: Stored NitroFuel");
            }
        }

        private void ActivateNitro()
        {
            if (!TryInitializeHalalitReferences())
            {
                Debug.LogError("UtilityManager: Cannot activate Nitro - Failed to find required components");
                return;
            }

            _isNitroActive = true;
            _hasNitroFuel = false;
            _nitroEndTime = Time.time + DURATION;
            
            _halalitMovement.SetSpeedLimit(_originalSpeedLimit * SPEED_MULTIPLIER);
            _halalitMovement.SetForceMultiplier(_originalForceMultiplier * FORCE_MULTIPLIER);
            
            Debug.Log($"UtilityManager: Activated Nitro. Speed limit increased to {_originalSpeedLimit * SPEED_MULTIPLIER}, Force increased to {_originalForceMultiplier * FORCE_MULTIPLIER}");
        }

        private void DeactivateNitro()
        {
            if (_halalitMovement != null)
            {
                _isNitroActive = false;
                _halalitMovement.SetSpeedLimit(_originalSpeedLimit);
                _halalitMovement.SetForceMultiplier(_originalForceMultiplier);
                Debug.Log($"UtilityManager: Deactivated Nitro. Speed and force reset to original values");
            }
        }
    }
} 