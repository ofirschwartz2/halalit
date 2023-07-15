using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

class AmmoToggle : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Joystick _ammoToggleJoystick;
    [SerializeField]
    private float _ammoToggleJoysticEdge;
    [SerializeField]
    private List<GameObject> _ammoPrefabs;

    private int _currentAmmoIndex;
    private bool _loadingNewAmmo;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _currentAmmoIndex = 0; 
    }

    void Update()
    {
        if (ShouldSwitchAmmoUp()) 
        {
            SwitchAmmoUp();
        }
        else if (ShouldSwitchAmmoDown())
        {
            SwitchAmmoDown();
        }

        if (IsNewAmmoLoaded())
        {
            FinishLoadingNewAmmo();
        }
    }

    public GameObject GetAmmoPrefab()
    {
        return _ammoPrefabs[_currentAmmoIndex];
    }

    private void SwitchAmmoUp()
    {
        if (_currentAmmoIndex == _ammoPrefabs.Count - 1)
        {
            _currentAmmoIndex = 0;
        }
        else
        {
            _currentAmmoIndex++;
        }

        LoadNewAmmo();
    }

    private void SwitchAmmoDown()
    {
        if (_currentAmmoIndex == 0)
        {
            _currentAmmoIndex = _ammoPrefabs.Count - 1;
        }
        else
        {
            _currentAmmoIndex--;
        }

        LoadNewAmmo();
    }

    private bool ShouldSwitchAmmoUp()
    {
        return !_loadingNewAmmo && _ammoToggleJoystick.Vertical > _ammoToggleJoysticEdge;
    }

    private bool ShouldSwitchAmmoDown()
    {
        return !_loadingNewAmmo && _ammoToggleJoystick.Vertical < _ammoToggleJoysticEdge * (-1);
    }

    private bool IsNewAmmoLoaded()
    {
        return _ammoToggleJoystick.Vertical == 0;
    }

    private void LoadNewAmmo()
    {
        _loadingNewAmmo = true;
        // TODO (dev): make handle untouchable but still returning to the center
    }

    private void FinishLoadingNewAmmo()
    {
        _loadingNewAmmo = false;
        // TODO (dev): make handle touchable again
    }
}
