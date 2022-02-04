using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{

    public bool UseConfigFile;
    public float CoolDownInterval = 0.5f;
    public List<GameObject> ShotsPrefab;
    public GameObject Halalit;
    public Joystick GunJoystick;
    public Joystick WeaponSwitchJoystick;

    private int _currentWeapon = 0;
    private float CooldownTime = 0;
    private bool _canSwitch = true;
    private Transform _firePoint;
    private GameObject ShotPrefab;
    private const float RADIUS = 0.65f;
    private const float SHOOTING_TH = 0.8f;
    private const float SWITCH_GUN_TH = 0.8f;

    void Start()
    {
        ShotPrefab = ShotsPrefab[_currentWeapon];
        if (UseConfigFile)
        {
            string[] props = {"CoolDownInterval"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            CoolDownInterval = propsFromConfig["CoolDownInterval"];
        }
        _firePoint = transform;
    }

    void Update()
    {
        if (CoolDownPassed() && JoystickShooting())
            Shoot();
        if(GunJoystickPressed())
            ChangeGunPosition();

        if(ShouldSwitchGunUp())
            SwitchGunUp();
        else if(ShouldSwitchGunDown())
            SwitchGunDown();
        else if (IsUnderSwitchGunTH())
            _canSwitch = true;
    }

    private bool GunJoystickPressed()
    {
        return GunJoystick.Horizontal != 0 && GunJoystick.Vertical != 0;
    }

    private void ChangeGunPosition()
    {
        float angle = GetAngle();
        _firePoint.position = Utils.AngleAndRadiusToPointOnCircle(angle, RADIUS) + Halalit.transform.position;  //new Vector2(x, y);
        _firePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private float GetAngle()
    {
        Vector2 input = new Vector2(GunJoystick.Horizontal, GunJoystick.Vertical);
        float angle;
        if (GunJoystick.Vertical < 0)
            angle = Vector2.Angle(input, Vector2.left) + 180f;
        else
            angle = Vector2.Angle(input, Vector2.right);
        return angle;
    }

    private bool CoolDownPassed()
    {
        return Time.time >= CooldownTime;
    }

    private bool JoystickShooting()
    {
        return Utils.GetLengthOfLine(GunJoystick.Horizontal, GunJoystick.Vertical) > SHOOTING_TH;
    }

    private bool ShouldSwitchGunUp()
    {
        return _canSwitch && WeaponSwitchJoystick.Vertical > SWITCH_GUN_TH;
    }

    private bool ShouldSwitchGunDown()
    {
        return _canSwitch && WeaponSwitchJoystick.Vertical < SWITCH_GUN_TH * (-1);
    }

    private bool IsUnderSwitchGunTH()
    {
        return Mathf.Abs(WeaponSwitchJoystick.Vertical) < SWITCH_GUN_TH;
    }

    private void Shoot() 
    {
        // Debug.Log("FirePoint.position " + _firePoint.position + ", _firePoint.rotation " + _firePoint.rotation);
        Instantiate(ShotPrefab, _firePoint.position, _firePoint.rotation);
        CooldownTime = Time.time + CoolDownInterval;
    }

    private void SwitchGunUp()
    {
        if (_currentWeapon == ShotsPrefab.Count - 1)
            _currentWeapon = 0;
        else
            _currentWeapon++;
        ShotPrefab = ShotsPrefab[_currentWeapon];
        _canSwitch = false;
    }

    private void SwitchGunDown()
    {
        if (_currentWeapon == 0)
            _currentWeapon = ShotsPrefab.Count - 1; 
        else
            _currentWeapon--;
        ShotPrefab = ShotsPrefab[_currentWeapon];    
        _canSwitch = false;
    }

    #region Items

    public void FasterCooldownInterval(float cooldownMutiplier)
    {
        CoolDownInterval *= cooldownMutiplier;
    }

    #endregion
}
