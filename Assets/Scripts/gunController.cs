using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunController : MonoBehaviour
{
    public Transform firePoint;
    public GameObject shotPrefab;
    public Joystick gunJoystick;
    public float cooldownTime = 0;
    public float cooldownInterval = 1;
    public static float radius = 2.6f;
    public float shootingTH = 0.8f;


    void Update()
    {
        if (coolDownPassed() && joystickShouldShoot()){
            shoot();
        }
        if(gunJoystickPressed()){
            gunLocation();
        }
    }
    bool gunJoystickPressed(){
        return (gunJoystick.Horizontal != 0 && gunJoystick.Vertical != 0);
    }
    void gunLocation(){
        float x = radius  * Mathf.Cos(getAngle() * Mathf.PI/180);
        float y = radius  * Mathf.Sin(getAngle() * Mathf.PI/180);
        // TODO: understand what is 11.05f. I don't understand...
        firePoint.position = new Vector2(x / 11f * 2.6f, y/ 11f * 2.6f);
        firePoint.rotation=  Quaternion.AngleAxis(getAngle(), Vector3.forward);
    }
    float getAngle(){
        Vector2 input = new Vector2(gunJoystick.Horizontal, gunJoystick.Vertical);
        float angle;
        if(gunJoystick.Vertical < 0)
            angle = Vector2.Angle(input, Vector2.left) + 180f;
        else
            angle = Vector2.Angle(input, Vector2.right);
        return angle;
    }
    bool coolDownPassed(){
        return (Time.time >= cooldownTime);
    }
    bool joystickShouldShoot(){
        return (Mathf.Sqrt(Mathf.Pow(gunJoystick.Horizontal, 2) + Mathf.Pow(gunJoystick.Vertical, 2))  > shootingTH);
    }
    void shoot() {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        cooldownTime = Time.time + cooldownInterval;
    }
}
