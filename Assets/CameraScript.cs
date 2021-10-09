using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    
    public Camera MainCam;
    public GameObject halalit;
    private void FixedUpdate() {
         MainCam.transform.position = new Vector3(halalit.transform.position.x, halalit.transform.position.y, MainCam.transform.position.z);
    }
}
