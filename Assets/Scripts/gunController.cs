using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{
    const float RADIUS = 2.6f;
    const float SHOOTING_TH = 0.8f;
    const float COOL_DOWN_INTERVAL = 0.5f;
    public float cooldownTime = 0;
    public Transform firePoint;
    public GameObject shotPrefab;
    public Joystick gunJoystick;


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
        float x = GetXFromAngle(RADIUS, angle);
        float y = GetYFromAngle(RADIUS, angle);
        firePoint.position = new Vector2(x, y);
        firePoint.rotation = Quaternion.AngleAxis(GetAngle(), Vector3.forward);
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
        return Time.time >= cooldownTime;
    }

    private bool JoystickShooting()
    {
        return LengthOfLine(gunJoystick.Horizontal, gunJoystick.Vertical) > SHOOTING_TH;
    }

    private void Shoot() 
    {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        cooldownTime = Time.time + COOL_DOWN_INTERVAL;
    }

    private float LengthOfLine(float x, float y)
    {
        return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
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
