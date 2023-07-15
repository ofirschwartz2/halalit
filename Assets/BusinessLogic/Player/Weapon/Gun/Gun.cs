using Assets.Utils;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private float _coolDownInterval;
    [SerializeField]
    private float _attackJoystickEdge;

    private float _cooldownTime;
    private GameObject _shotPrefab;
    private AmmoToggle _ammoToggle;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _ammoToggle = gameObject.GetComponentInParent<AmmoToggle>();
        _cooldownTime = 0;
    }

    void Update()
    {
        _shotPrefab = _ammoToggle.GetAmmoPrefab();

        if (ShouldFire() && IsCoolDownPassed())
        {
            Fire();
        }
    }

    private void Fire()
    {
        Instantiate(_shotPrefab, transform.position, transform.rotation);
        _cooldownTime = Time.time + _coolDownInterval;
    }

    #region Predicates
    private bool IsCoolDownPassed()
    {
        return Time.time >= _cooldownTime;
    }

    private bool ShouldFire()
    {
        return Utils.GetLengthOfLine(_attackJoystick.Horizontal, _attackJoystick.Vertical) > _attackJoystickEdge;
    }
    #endregion

    void FasterCooldownInterval(float cooldownMutiplier) // TODO (refactor): move to ItemLoad
    {
        _coolDownInterval *= cooldownMutiplier;
    }
}
