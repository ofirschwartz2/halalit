using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackShotScript : MonoBehaviour
{
    public bool UseConfigFile;
    public float Speed, Lifetime;
    
    private List<string> _tagsAffected;
    private Rigidbody2D _rigidBody;
    private float _endOfLiveTime = 0;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
            ConfigureFromFile();
        
        _tagsAffected = new List<string>{"Enemy"};
        _endOfLiveTime = Time.time + Lifetime;
    }

    void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x * Speed,transform.localScale.y * Speed, 1);
        if(ShotDied())
            Destroy(gameObject);
    }

    private bool ShotDied()
    {
        return Time.time >= _endOfLiveTime;
    }

    void ConfigureFromFile()
    {
        string[] props = { "Speed", "Lifetime" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

        Speed = propsFromConfig["Speed"];
        Lifetime = propsFromConfig["Lifetime"];
    }
}
