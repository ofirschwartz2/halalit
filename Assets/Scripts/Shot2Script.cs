using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot2Script : MonoBehaviour
{
    public bool UseConfigFile;
    public float Lifetime;
    private float endOfLiveTime = 0;


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = { "Lifetime" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            Lifetime = propsFromConfig["Lifetime"];
        }
        endOfLiveTime = Time.time + Lifetime;
    }

    void Update()
    {
        if (CoolDownPassed())
            Destroy(gameObject);
    }

    private bool CoolDownPassed()
    {
        return Time.time >= endOfLiveTime;
    }
}
