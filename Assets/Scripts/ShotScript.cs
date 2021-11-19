using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    public bool UseConfigFile;
    public float Speed;
    public Rigidbody2D RigidBody;

    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = { "Speed" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            Speed = propsFromConfig["Speed"];
        }

        RigidBody.velocity = transform.right * Speed;
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }

}
