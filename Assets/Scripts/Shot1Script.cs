using Assets.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot1Script : MonoBehaviour
{
    public bool UseConfigFile;
    public float Speed;
    private Rigidbody2D _rigidBody;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
        {
            string[] props = { "Speed" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            Speed = propsFromConfig["Speed"];
        }
        _rigidBody.velocity = transform.right * Speed;
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.tag != "PickupClaw" && other.tag != "OutOfScreen" && other.tag != "Shot")
                Destroy(gameObject);
    }

}
