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
    public Transform firePoint;
    public List<GameObject> shotsPrefab;
    private int currentWeapon = 0;
    private bool canSwitch = true;
    private GameObject shotPrefab;
    // TODO: the gun should not hold it's parent (halalit), it's a bad practice, it only need it's x and y positions, so it should use a function of the halalitMovementController that will get them.
    public GameObject halalit;
    public Joystick gunJoystick;
    public Joystick weaponSwitchJoystick;

    void Start()
    {
        shotPrefab = shotsPrefab[currentWeapon];
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
            canSwitch = true;
    }

    private bool GunJoystickPressed()
    {
        return gunJoystick.Horizontal != 0 && gunJoystick.Vertical != 0;
    }

    private void ChangeGunPosition()
    {
        float angle = GetAngle();
        firePoint.position = Utils.AngleAndRadiusToPointOnCircle(angle, RADIUS) + halalit.transform.position;  //new Vector2(x, y);
        firePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private float GetAngle()
    {
        Vector2 input = new Vector2(gunJoystick.Horizontal, gunJoystick.Vertical);
        float angle;
        if (gunJoystick.Vertical < 0)
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
        return Utils.GetLengthOfLine(gunJoystick.Horizontal, gunJoystick.Vertical) > SHOOTING_TH;
    }

    private bool ShouldSwitchGunUp()
    {
        return canSwitch && weaponSwitchJoystick.Vertical > SWITCH_GUN_TH;
    }

    private bool ShouldSwitchGunDown()
    {
        return canSwitch && weaponSwitchJoystick.Vertical < SWITCH_GUN_TH * (-1);
    }

    private bool IsUnderSwitchGunTH()
    {
        return Mathf.Abs(weaponSwitchJoystick.Vertical) < SWITCH_GUN_TH;
    }

    private void Shoot() 
    {
        // Debug.Log("firePoint.position " + firePoint.position + ", firePoint.rotation " + firePoint.rotation);
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        CooldownTime = Time.time + COOL_DOWN_INTERVAL;
    }

    private void SwitchGunUp()
    {
        if (currentWeapon == shotsPrefab.Count - 1)
            currentWeapon = 0;
        else
            currentWeapon++;
        shotPrefab = shotsPrefab[currentWeapon];
        canSwitch = false;
    }

    private void SwitchGunDown()
    {
        if (currentWeapon == 0)
            currentWeapon = shotsPrefab.Count - 1; 
        else
            currentWeapon--;
        shotPrefab = shotsPrefab[currentWeapon];    
        canSwitch = false;
    }
}
