using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{
    private const float RADIUS = 2.6f;
    private const float SHOOTING_TH = 0.8f;
    private const float COOL_DOWN_INTERVAL = 0.5f;

    public bool UseConfigFile;
    public float CooldownTime = 0;
    public Transform firePoint;
    public GameObject shotPrefab;
    // TODO: the gun should not hold it's parent (halalit), it's a bad practice, it only need it's x and y positions, so it should use a function of the halalitMovementController that will get them.
    public GameObject halalit;
    public Joystick gunJoystick;

    void Start()
    {
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
    }

    private bool GunJoystickPressed()
    {
        return gunJoystick.Horizontal != 0 && gunJoystick.Vertical != 0;
    }

    private void ChangeGunPosition()
    {
        float angle = GetAngle();
        float x = GetXFromAngle(RADIUS, angle) + halalit.transform.position.x;
        float y = GetYFromAngle(RADIUS, angle) + halalit.transform.position.y;
        firePoint.position = new Vector2(x, y);
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

    private void Shoot() 
    {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        CooldownTime = Time.time + COOL_DOWN_INTERVAL;
    }

    private float GetXFromAngle(float radius, float angle)
    {
        // TODO: understand what is 11f. I don't understand...
        return radius * Mathf.Cos(angle * Mathf.PI/180) / 11f * radius;
    }

    private float GetYFromAngle(float radius, float angle)
    {
        // TODO: understand what is 11f. I don't understand...
        return radius * Mathf.Sin(angle * Mathf.PI/180) / 11f * radius;
    }
}
