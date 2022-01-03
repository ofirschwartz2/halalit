using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public bool UseConfigFile;
    public Camera MainCam;
    public GameObject halalit;
    public float UpperEdge, LowerEdge, RightEdge, LeftEdge;


    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        if (UseConfigFile)
        {
            string[] props = { "UpperEdge", "LowerEdge", "RightEdge", "LeftEdge" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            UpperEdge = propsFromConfig["UpperEdge"];
            LowerEdge = propsFromConfig["LowerEdge"];
            RightEdge = propsFromConfig["RightEdge"];
            LeftEdge = propsFromConfig["LeftEdge"];
        }
    }

    private void FixedUpdate() 
    {
        float newXPos = transform.position.x;
        float newYPos = transform.position.y;

        if (isValidHorizontalPosition())
            newXPos = halalit.transform.position.x;

        if (isValidVerticalPosition())
            newYPos = halalit.transform.position.y;

        MainCam.transform.position = new Vector3(newXPos, newYPos, transform.position.z);
    }

    private bool isValidHorizontalPosition()
    {
        return halalit.transform.position.x > LeftEdge && halalit.transform.position.x < RightEdge;
    }

    private bool isValidVerticalPosition()
    {
        return halalit.transform.position.y > LowerEdge && halalit.transform.position.y < UpperEdge;
    }
}
