using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{
    private const float RADIUS = 0.65f;
    private const float SHOOTING_TH = 0.8f;
    private const float SWITCH_GUN_TH = 0.8f;
    private const float COOL_DOWN_INTERVAL = 0.5f;

    public bool UseConfigFile;
    public float CooldownTime = 0;
    public Transform FirePoint;
    public List<GameObject> ShotsPrefab;
    public GameObject Halalit;
    public Joystick GunJoystick;
    public Joystick WeaponSwitchJoystick;
    private int _currentWeapon = 0;
    private bool _canSwitch = true;
    private GameObject ShotPrefab;


    void Start()
    {
        ShotPrefab = ShotsPrefab[_currentWeapon];
        if (UseConfigFile)
        {
            string[] props = { "CooldownTime" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            CooldownTime = propsFromConfig["CooldownTime"];
        }
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
        FirePoint.position = Utils.AngleAndRadiusToPointOnCircle(angle, RADIUS) + Halalit.transform.position;  //new Vector2(x, y);
        FirePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
        // Debug.Log("FirePoint.position " + FirePoint.position + ", firePoint.rotation " + firePoint.rotation);
        Instantiate(ShotPrefab, FirePoint.position, FirePoint.rotation);
        CooldownTime = Time.time + COOL_DOWN_INTERVAL;
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
}
