using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot2Script : MonoBehaviour
{
    public bool UseConfigFile;
    public float Lifetime;
    private float _endOfLiveTime = 0;

    public LayerMask IgnoreMe;


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = { "Lifetime" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            Lifetime = propsFromConfig["Lifetime"];
        }
        Vector3 hitPoint = CalculateHitPoint();
        Debug.Log(hitPoint);
        _endOfLiveTime = Time.time + Lifetime;
    }

    void Update()
    {
        if (ShotDied())
            Destroy(gameObject);
    }

    private bool ShotDied()
    {
        return Time.time >= _endOfLiveTime;
    }

    private Vector3 CalculateHitPoint()
    {
        RaycastHit hit;
        Debug.Log("position" + transform.position);
        Debug.Log("rotation" + Utils.QuaterionToVector(transform.rotation));
        Physics.Raycast(transform.position, Utils.QuaterionToVector(transform.rotation), out hit, ~IgnoreMe);
        Debug.Log(hit.collider);
        return hit.point;
    }
}
