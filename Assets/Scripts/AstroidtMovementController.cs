using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;


public class AstroidtMovementController : MonoBehaviour
{

    public bool UseConfigFile;
    public float MaxXSpeed, MaxYSpeed, MinRotation, MaxRotation;
    private float _rotationSpeed;


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MaxXSpeed", "MaxYSpeed", "MinRotation", "MaxRotation"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
            MinRotation = propsFromConfig["MinRotation"];
            MaxRotation = propsFromConfig["MaxRotation"];
        }

        GetComponent<Rigidbody2D>().velocity = GetVelocity(MaxXSpeed, MaxYSpeed);
        _rotationSpeed = GetRotationSpeed(MaxRotation* (-1), MaxRotation);
    }

    void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        transform.Rotate(0, 0, _rotationSpeed, Space.Self);
    }   

    private float GetRotationSpeed(float minRotation, float maxRotation)
    {
        return UnityEngine.Random.Range(minRotation, maxRotation);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("OutOfScreen"))
            Destroy(gameObject);
    }

    Vector2 GetVelocity(float maxXSpeed, float maxYSpeed)
    {
        return new Vector2(GetXVelocity(maxXSpeed), GetYVelocity(maxYSpeed) );
    }

    private float GetXVelocity(float maxXSpeed) 
    {
        if (transform.position.x > 0)
            return UnityEngine.Random.Range(maxXSpeed * (-1), 0f);
        else if (transform.position.x < 0)
            return UnityEngine.Random.Range(0f, maxXSpeed);
        return UnityEngine.Random.Range(maxXSpeed * (-1), maxXSpeed);
    }

    private float GetYVelocity(float maxYSpeed) 
    {
        if (transform.position.y > 0)
            return UnityEngine.Random.Range(maxYSpeed * (-1), 0f);
        else if (transform.position.y < 0)
            return UnityEngine.Random.Range(0f, maxYSpeed);
        return UnityEngine.Random.Range(maxYSpeed * (-1), maxYSpeed);
    }

}