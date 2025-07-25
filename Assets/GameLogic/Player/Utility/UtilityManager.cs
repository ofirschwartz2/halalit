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
        private IUtility _pendingUtility;
        private GameObject _halalit;

        private void Awake()
        {
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

            _halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
            if (_halalit == null)
            {
                return false;
            }

            return true;
        }

        private void SetEventListeners()
        {
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
            if (_currentUtility?.IsActive == true)
            {
                bool shouldDeactivate = false;
                if (_currentUtility is NitroUtility nitro)
                    shouldDeactivate = nitro.ShouldDeactivate();
                else if (_currentUtility is PushingMagnetUtility magnet)
                    shouldDeactivate = magnet.ShouldDeactivate();
                else if (_currentUtility is PullingMagnetUtility pullingMagnet)
                    shouldDeactivate = pullingMagnet.ShouldDeactivate();

                if (shouldDeactivate)
                {
                    _currentUtility.Deactivate();
                    HandleUtilityDeactivation();
                }
            }
        }

        private void HandleUtilityDeactivation()
        {
            if (_pendingUtility != null)
            {
                _currentUtility = _pendingUtility;
                _pendingUtility = null;
                UpdateUtilityButtonState();
            }
            else
            {
                _currentUtility = null;
                utilityButton.ClearUtility();
            }
        }

        private void OnUtilityButtonClicked()
        {
            if (_currentUtility != null && _currentUtility.CanActivate() && TryInitializeHalalitReference())
            {
                _currentUtility.Activate(_halalit);
                UpdateUtilityButtonState();
            }
        }

        private void OnUtilityPickup(object initiator, ItemEventArguments arguments)
        {
            
            try
            {
                var newUtility = _utilityFactory.CreateUtility(arguments.Name);
                
                if (_currentUtility?.IsActive == true)
                {
                    _pendingUtility = newUtility;
                    UpdateUtilityButtonState();
                    return;
                }
                
                _currentUtility = newUtility;
                UpdateUtilityButtonState();
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"UtilityManager: {e.Message}");
            }
        }

        private void UpdateUtilityButtonState()
        {
            if (utilityButton == null) return;

            if (_currentUtility?.IsActive == true)
            {
                // If there's a pending utility, show it but disabled
                if (_pendingUtility != null)
                {
                    utilityButton.SetUtilityText(GetUtilityButtonText(_pendingUtility));
                    utilityButton.SetInteractable(false);
                }
                else
                {
                    utilityButton.ClearUtility();
                }
            }
            else if (_currentUtility != null)
            {
                utilityButton.SetUtilityText(GetUtilityButtonText(_currentUtility));
                utilityButton.SetInteractable(true);
            }
            else
            {
                utilityButton.ClearUtility();
            }
        }

        private string GetUtilityButtonText(IUtility utility)
        {
            if (utility is NitroUtility)
            {
                return "NF";
            }
            if (utility is PushingMagnetUtility)
            {
                return "M>";
            }
            if (utility is PullingMagnetUtility)
            {
                return "M<";
            }
            return "??";
        }
    }
} 