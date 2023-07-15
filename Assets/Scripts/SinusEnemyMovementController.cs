using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SinusEnemyMovementController : MonoBehaviour
{
    public float XVelocity, YVelocity, XSpeedLimit, YSpeedLimit;
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
        _changeXForceDirectionInterval = 1;
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
        _rigidBody.AddForce(_direction * XVelocity);
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
            _rigidBody.AddForce(UP * YVelocity);
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
        string[] props = { "XVelocity", "YVelocity", "XSpeedLimit", "YSpeedLimit"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        XVelocity = propsFromConfig["XVelocity"];
        YVelocity = propsFromConfig["YVelocity"];
        XSpeedLimit = propsFromConfig["XSpeedLimit"];
        YSpeedLimit = propsFromConfig["YSpeedLimit"];
    }
}
