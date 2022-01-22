using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public bool UseConfigFile;
    public Camera MainCam;
    public GameObject Halalit;
    public GameObject Background;
    public float CameraXSize, CameraYSize;
    private float _leftSceneEdge, _rightSceneEdge, _topSceneEdge, _bottomSceneEdge;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        if (UseConfigFile)
        {
            string[] props = {"CameraXSize", "CameraYSize" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            CameraXSize = propsFromConfig["CameraXSize"];
            CameraYSize = propsFromConfig["CameraYSize"];
        }

        SetBackgroundSizes();        
    }

    private void FixedUpdate() 
    {
        float newXPos = transform.position.x;
        float newYPos = transform.position.y;

        if (isValidHorizontalPosition())
            newXPos = Halalit.transform.position.x;

        if (isValidVerticalPosition())
            newYPos = Halalit.transform.position.y;

        MainCam.transform.position = new Vector3(newXPos, newYPos, transform.position.z);
    }

    private bool isValidHorizontalPosition()
    {
        return 
            Halalit.transform.position.x > (_leftSceneEdge + (CameraXSize / 2)) && 
            Halalit.transform.position.x < (_rightSceneEdge - (CameraXSize / 2));
    }

    private bool isValidVerticalPosition()
    {
        return 
        Halalit.transform.position.y > (_bottomSceneEdge + (CameraYSize / 2)) && 
        Halalit.transform.position.y < (_topSceneEdge - (CameraYSize / 2));
    }

    private void SetBackgroundSizes()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;

        _rightSceneEdge = bgSize.x / 2; 
        _leftSceneEdge = _rightSceneEdge * (-1);
        _topSceneEdge = bgSize.y / 2;
        _bottomSceneEdge = _topSceneEdge * (-1);
    }
}
