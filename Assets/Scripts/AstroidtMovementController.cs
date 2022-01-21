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
    public float MinRotation;
    public float MaxRotation;
    private float _rotationSpeed;

    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed", "MinRotation", "MaxRotation"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
            MinRotation = propsFromConfig["MinRotation"];
            MaxRotation = propsFromConfig["MaxRotation"];
        }

        GetComponent<Rigidbody2D>().velocity = GetVelocity(MinXSpeed, MaxXSpeed, MinYSpeed, MaxYSpeed);
        _rotationSpeed = GetRotationSpeed(MinRotation, MaxRotation);
    }

    void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        transform.Rotate(0, 0, _rotationSpeed, Space.Self);
    }   

    Vector2 GetVelocity(float minXSpeed, float maxXSpeed, float minYSpeed, float maxYSpeed)
    {
        return new Vector2(UnityEngine.Random.Range(MinXSpeed, MaxXSpeed), UnityEngine.Random.Range(MinYSpeed, MaxYSpeed));
    }

    private float GetRotationSpeed(float minRotation, float maxRotation)
    {
        return UnityEngine.Random.Range(minRotation, maxRotation);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot") || other.gameObject.CompareTag("Background"))
            Destroy(gameObject);
    }
}