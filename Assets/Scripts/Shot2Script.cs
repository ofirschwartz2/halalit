using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot2Script : MonoBehaviour
{
    public bool UseConfigFile;
    public float Lifetime;
    public float EndOfLiveTime = 0;


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = { "Lifetime" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            Lifetime = propsFromConfig["Lifetime"];
        }
        EndOfLiveTime = Time.time + Lifetime;
    }

    void Update()
    {
        if (CoolDownPassed())
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        Destroy(gameObject);
    }

    private bool CoolDownPassed()
    {
        return Time.time >= EndOfLiveTime;
    }
}
