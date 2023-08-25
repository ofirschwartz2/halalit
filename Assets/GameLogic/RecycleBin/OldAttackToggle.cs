//using Assets.Utils;
//using System.Collections.Generic;
//using UnityEngine;

//class OldAttackToggle : MonoBehaviour
//{
//    [SerializeField]
//    private bool _useConfigFile;
//    [SerializeField]
//    private Joystick _attackToggleJoystick;
//    [SerializeField]
//    private float _attackToggleJoysticEdge;
//    [SerializeField]
//    private List<GameObject> _attackPrefabs;

//    private int _currentAttackIndex;
//    private bool _loadingNewAttack;

//    #region Init
//    private void Awake()
//    {
//        SetEventListeners();
//    }

//    private void SetEventListeners()
//    {
//        AttackEvent.NewAttackSwitch += NewAttackSwitch;
//    }

//    void Start()
//    {
//        if (_useConfigFile)
//        {
//            ConfigFileReader.LoadMembersFromConfigFile(this);
//        }

//        _currentAttackIndex = 0;
//    }
//    #endregion

//    #region Accessors
//    public GameObject GetAttackPrefab()
//    {
//        return _attackPrefabs[_currentAttackIndex];
//    }
//    #endregion

//    #region Logic
//    void Update()
//    {
//        if (ShouldSwitchAttackUp())
//        {
//            SwitchAttackUp();
//        }
//        else if (ShouldSwitchAttackDown())
//        {
//            SwitchAttackDown();
//        }

//        if (IsNewAttackLoaded())
//        {
//            FinishLoadingNewAttack();
//        }
//    }

//    private void SwitchAttackUp()
//    {
//        if (_currentAttackIndex == _attackPrefabs.Count - 1)
//        {
//            _currentAttackIndex = 0;
//        }
//        else
//        {
//            _currentAttackIndex++;
//        }

//        LoadNewAttack();
//    }

//    private void SwitchAttackDown()
//    {
//        if (_currentAttackIndex == 0)
//        {
//            _currentAttackIndex = _attackPrefabs.Count - 1;
//        }
//        else
//        {
//            _currentAttackIndex--;
//        }

//        LoadNewAttack();
//    }

//    private void LoadNewAttack()
//    {
//        _loadingNewAttack = true;
//    }

//    private void FinishLoadingNewAttack()
//    {
//        _loadingNewAttack = false;
//    }
//    #endregion

//    #region Event Actions
//    private void NewAttackSwitch(object initiator, AttackEventArguments arguments)
//    {
//        _attackPrefabs.Add(arguments.AttackPrefab);
//        Debug.Log("New attack - " + arguments.AttackName.ToString() + " loaded");
//    }
//    #endregion

//    #region Predicates
//    private bool ShouldSwitchAttackUp()
//    {
//        return !_loadingNewAttack && _attackToggleJoystick.Vertical > _attackToggleJoysticEdge;
//    }

//    private bool ShouldSwitchAttackDown()
//    {
//        return !_loadingNewAttack && _attackToggleJoystick.Vertical < _attackToggleJoysticEdge * (-1);
//    }

//    private bool IsNewAttackLoaded()
//    {
//        return _attackToggleJoystick.Vertical == 0;
//    }
//    #endregion
//}
