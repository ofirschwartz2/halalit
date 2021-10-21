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
    void Update()
    {
        Debug.Log("ANGLE IS: "+ getAngle());
        //Debug.Log("HORIZONTAL: " + gunJoystick.Horizontal + ", VERTICAL: " + gunJoystick.Vertical);
        if (coolDownPassed() && joystickShouldShoot()){
            shoot();
        }
    }
    float getAngle(){
        if(gunJoystick.Vertical > 0){
            if(gunJoystick.Horizontal > 0) {
                return (Mathf.Asin(gunJoystick.Vertical / gunJoystick.Horizontal) * 180/Mathf.PI);
            } else{
                return (180 - (Mathf.Asin(gunJoystick.Vertical / (gunJoystick.Horizontal * -1)) * 180/Mathf.PI));
            }
        } else{
            if(gunJoystick.Horizontal < 0) {
                return (180 + (Mathf.Asin((gunJoystick.Vertical * -1) / (gunJoystick.Horizontal * -1)) * 180/Mathf.PI));
            } else{
                return (360 - (Mathf.Asin((gunJoystick.Vertical * -1) / gunJoystick.Horizontal) * 180/Mathf.PI));
            }
        }
    }
    bool coolDownPassed(){
        if(Time.time >= cooldownTime){
            return true;
        } 
        return false;
    }
    bool joystickShouldShoot(){
        //Debug.Log("SHOULDSHOOT: " + Mathf.Sqrt(Mathf.Pow(gunJoystick.Horizontal, 2) + Mathf.Pow(gunJoystick.Vertical, 2)));
        if (Mathf.Sqrt(Mathf.Pow(gunJoystick.Horizontal, 2) + Mathf.Pow(gunJoystick.Vertical, 2))  > 0.9){
            return true;
        }
        return false;
    }
    void shoot() {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        cooldownTime = Time.time + cooldownInterval;
    }
}
