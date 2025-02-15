using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using Items.Factory;
using Items.Utility;
using UnityEngine;

namespace Assets.Player
{
    public class UtilityManager : MonoBehaviour
    {
        private UtilityFactory _utilityFactory;
        private IUtility _currentUtility;
        private GameObject _halalit;

        private void Awake()
        {
            Debug.Log("UtilityManager: Awake");
            _utilityFactory = new UtilityFactory();
            SetEventListeners();
            TryInitializeHalalitReference();
        }

        private bool TryInitializeHalalitReference()
        {
            if (_halalit != null) return true;

            Debug.Log("UtilityManager: Trying to initialize Halalit reference");
            _halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
            if (_halalit == null)
            {
                Debug.LogWarning("UtilityManager: Could not find Halalit GameObject");
                return false;
            }

            Debug.Log("UtilityManager: Successfully initialized Halalit reference");
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
            if (_currentUtility?.IsActive == true)
            {
                _currentUtility.Deactivate();
            }
        }

        private void DestroyEventListeners()
        {
            ItemEvent.PlayerUtilityPickUp -= OnUtilityPickup;
        }

        private void Update()
        {
            if (_currentUtility?.IsActive == true && _currentUtility is NitroUtility nitro && nitro.ShouldDeactivate())
            {
                _currentUtility.Deactivate();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("UtilityManager: U key pressed");
                if (_currentUtility != null && _currentUtility.CanActivate())
                {
                    if (TryInitializeHalalitReference())
                    {
                        _currentUtility.Activate(_halalit);
                    }
                }
                else
                {
                    Debug.Log("UtilityManager: No utility to activate or utility is already active");
                }
            }
        }

        private void OnUtilityPickup(object initiator, ItemEventArguments arguments)
        {
            Debug.Log($"UtilityManager: Received pickup event for {arguments.Name}");
            
            try
            {
                var newUtility = _utilityFactory.CreateUtility(arguments.Name);
                if (_currentUtility?.IsActive == true)
                {
                    Debug.Log("UtilityManager: Current utility is active, not replacing it");
                    return;
                }
                
                _currentUtility = newUtility;
                Debug.Log($"UtilityManager: Stored new utility of type {arguments.Name}");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"UtilityManager: {e.Message}");
            }
        }
    }
} 