using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SinusEnemyMovementController : MonoBehaviour
{
    public float XForce, YForce, YSpeedLimit;
    public bool UseConfigFile;

    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private Vector2 UP = new Vector2(0, 1);
    private Vector2 RIGHT = new Vector2(1, 0);
    private Vector2 LEFT = new Vector2(-1, 0);
    private Vector2 _direction = new Vector2(0, -1);
    private float _changeXForceDirectionTime, _changeXForceDirectionInterval;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _direction = RIGHT;
        _changeXForceDirectionInterval = 2;
        UpdateChangeXForceDirectionTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "SinusEnemy";
    }

    void Update()
    {
        YMovement();
        XMovement();
    }
    
    private void XMovement()
    {
        if (DidXTimePass())
        {
            UpdateChangeXForceDirectionTime();
            XChangeDirection();
        }
        _rigidBody.AddForce(_direction * XForce);
    }

    private void XChangeDirection()
    {
        _direction = _direction == RIGHT ? LEFT : RIGHT;
        Debug.Log(_direction);
    }

    private bool DidXTimePass()
    {
        return Time.time > _changeXForceDirectionTime;
    }

    private void YMovement()
    {
        if (YIsUnderSpeedLimit())
        {
            _rigidBody.AddForce(UP * YForce);
        }
    }
    private bool YIsUnderSpeedLimit()
    {
        return _rigidBody.velocity.y < YSpeedLimit;
    }

    private void UpdateChangeXForceDirectionTime() 
    {
        _changeXForceDirectionTime = Time.time + _changeXForceDirectionInterval;
    }

    private void ConfigureFromFile()
    {
        string[] props = { "XForce", "YForce", "YSpeedLimit"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        XForce = propsFromConfig["XForce"];
        YForce = propsFromConfig["YForce"];
        YSpeedLimit = propsFromConfig["YSpeedLimit"];
    }
}
