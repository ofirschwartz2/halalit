using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using Items.Factory;
using Items.Utility;
using Meta.UI;
using UnityEngine;

namespace Assets.Player
{
    public class UtilityManager : MonoBehaviour
    {
        [SerializeField] private UtilityButton utilityButton;

        private UtilityFactory _utilityFactory;
        private IUtility _currentUtility;
        private GameObject _halalit;

        private void Awake()
        {
            Debug.Log("UtilityManager: Awake");
            _utilityFactory = new UtilityFactory();
            SetEventListeners();
            TryInitializeHalalitReference();

            if (utilityButton != null)
            {
                utilityButton.AddClickListener(OnUtilityButtonClicked);
            }
            else
            {
                Debug.LogError("UtilityManager: UtilityButton reference is missing!");
            }
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

            if (utilityButton != null)
            {
                utilityButton.RemoveClickListener(OnUtilityButtonClicked);
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
                utilityButton.ClearUtility();
            }
        }

        private void OnUtilityButtonClicked()
        {
            if (_currentUtility != null && _currentUtility.CanActivate() && TryInitializeHalalitReference())
            {
                _currentUtility.Activate(_halalit);
                utilityButton.ClearUtility();
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
                UpdateUtilityButtonText(arguments.Name);
                Debug.Log($"UtilityManager: Stored new utility of type {arguments.Name}");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"UtilityManager: {e.Message}");
            }
        }

        private void UpdateUtilityButtonText(ItemName utilityName)
        {
            if (utilityButton == null) return;

            string buttonText = utilityName switch
            {
                ItemName.NITRO_FUEL => "NF",
                _ => "??"
            };

            utilityButton.SetUtilityText(buttonText);
        }
    }
} 