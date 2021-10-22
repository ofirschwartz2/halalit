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
    public float circumference = 2 * radius * Mathf.PI;
    public float shootingTH = 0.8f;


    void Update()
    {
        //Debug.Log("HORIZONTAL: " + gunJoystick.Horizontal + ", VERTICAL: " + gunJoystick.Vertical);
        if (coolDownPassed() && joystickShouldShoot()){
            shoot();
        }
        gunLocation();
    }
    void gunLocation(){
        float x = radius  * Mathf.Cos(getAngle() * Mathf.PI/180);
        float y = radius  * Mathf.Sin(getAngle() * Mathf.PI/180);
        //Debug.Log("X: " + x + ", Y: " + y + ", Angle: " + getAngle() + ", Vertical: " + gunJoystick.Vertical + ", Horizontal: " + gunJoystick.Horizontal);
        // TODO: understand what is 11.05f. I don't understand...
        firePoint.position = new Vector2(x / 11f * 2.6f, y/ 11f * 2.6f);
        firePoint.rotation=  Quaternion.AngleAxis(getAngle(), Vector3.forward);
    }
    float getAngle(){
        Vector2 input = new Vector2(gunJoystick.Horizontal, gunJoystick.Vertical);
        float angle = Vector2.Angle(input, Vector2.right);
        if(gunJoystick.Vertical < 0)
                angle = Vector2.Angle(input, Vector2.left) + 180f;
        return angle;
        /*
        if(gunJoystick.Vertical > 0 && gunJoystick.Horizontal > 0){
            angle = 90 - angle;
        } else if (gunJoystick.Vertical < 0 && gunJoystick.Horizontal > 0){
            angle = 180 + angle;
        }
        
        Debug.Log("Angle: " +      angle ) ;  
        

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
        */
        
    }
    bool coolDownPassed(){
        if(Time.time >= cooldownTime){
            return true;
        } 
        return false;
    }
    bool joystickShouldShoot(){
        //Debug.Log("SHOULDSHOOT: " + Mathf.Sqrt(Mathf.Pow(gunJoystick.Horizontal, 2) + Mathf.Pow(gunJoystick.Vertical, 2)));
        if (Mathf.Sqrt(Mathf.Pow(gunJoystick.Horizontal, 2) + Mathf.Pow(gunJoystick.Vertical, 2))  > shootingTH){
            return true;
        }
        return false;
    }
    void shoot() {
        Instantiate(shotPrefab, firePoint.position, firePoint.rotation);
        cooldownTime = Time.time + cooldownInterval;
    }
}
