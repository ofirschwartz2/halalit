using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AstroidtMovementController : MonoBehaviour
{

    public bool UseConfigFile;
    public float MinXSpeed;
    public float MaxXSpeed;
    public float MinYSpeed;
    public float MaxYSpeed;

    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
        }
        
        GetComponent<Rigidbody2D>().velocity = GetAstroidtVelocity(MinXSpeed, MaxXSpeed, MinYSpeed, MaxYSpeed);
    }

    private Vector2 GetAstroidtVelocity(float minXSpeed, float maxXSpeed, float minYSpeed, float maxYSpeed)
    {
        return new Vector2(UnityEngine.Random.Range(MinXSpeed, MaxXSpeed), UnityEngine.Random.Range(MinYSpeed, MaxYSpeed));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot") || other.gameObject.CompareTag("Background"))
            Destroy(gameObject);
    }
}