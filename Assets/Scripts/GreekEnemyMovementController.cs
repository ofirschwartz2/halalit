using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GreekEnemyMovementController : MonoBehaviour
{
    public float Force;
    public bool UseConfigFile;

    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private VerticalGreekEnemyDirection _direction;
    private float _changeDirectionTime, _changeDirectionInterval, _speedUpSlowDownInterval;
    private bool _isSpeedingUp;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _direction = VerticalGreekEnemyDirection.UP1;
        _changeDirectionInterval = 2;
        _speedUpSlowDownInterval = (8/10) * _changeDirectionInterval;
        UpdateChangeDirectionTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "GreekEnemy"; // TODO: should this always be "Enemy" on the Enemy level?
    }

    void Update()
    {
        if (DidChangeDirectionTimePass())
        {
            ChangeDirection();
        }

        TryChangeSpeedUpSlowDown();

        Move();
    }

    private void TryChangeSpeedUpSlowDown()
    {
        if (ShouldSlowDown())
        {
            _isSpeedingUp = false;
        }
        else
        {
            _isSpeedingUp = true;
        }
    }

    private bool ShouldSlowDown()
    {
        return 
            _changeDirectionTime - Time.time  < _speedUpSlowDownInterval;
    }

    private void ChangeDirection()
    {
        _direction = GetNextDirection<VerticalGreekEnemyDirection>();
        _isSpeedingUp = true;
        UpdateChangeDirectionTime();
    }

    private void Move()
    {
        GreekMove(_direction, _isSpeedingUp);
    }

    private void GreekMove(VerticalGreekEnemyDirection direction,bool isSpeedingUp) 
    {
        var speedUpDlowDownNumber = isSpeedingUp ? 1 : -1;

        switch (direction)
        {
            case VerticalGreekEnemyDirection.UP1:
            case VerticalGreekEnemyDirection.UP2:
                _rigidBody.AddForce(Vector2.up * Force * speedUpDlowDownNumber);
                break;
            case VerticalGreekEnemyDirection.RIGHT:
                _rigidBody.AddForce(Vector2.right * Force * speedUpDlowDownNumber);
                break;
            case VerticalGreekEnemyDirection.LEFT:
                _rigidBody.AddForce(Vector2.left * Force * speedUpDlowDownNumber);
                break;
        }
    }

    private void GreekMove(HorizontalGreekEnemyDirection direction, bool isSpeedingUp)
    {

        var speedUpDlowDownNumber = isSpeedingUp ? 1 : -1;

        switch (direction)
        {
            case HorizontalGreekEnemyDirection.RIGHT1:
            case HorizontalGreekEnemyDirection.RIGHT2:
                _rigidBody.AddForce(Vector2.right * Force * speedUpDlowDownNumber);
                break;
            case HorizontalGreekEnemyDirection.UP:
                _rigidBody.AddForce(Vector2.up * Force * speedUpDlowDownNumber);
                break;
            case HorizontalGreekEnemyDirection.DOWN:
                _rigidBody.AddForce(Vector2.down * Force * speedUpDlowDownNumber);
                break;
        }
    }

    private bool DidChangeDirectionTimePass()
    {
        return Time.time > _changeDirectionTime;
    }

    private TGreekDirection GetNextDirection<TGreekDirection>()
    {
        TGreekDirection[] enumValues = (TGreekDirection[])Enum.GetValues(typeof(TGreekDirection));
        int currentIndex = Array.IndexOf(enumValues, _direction);
        int nextIndex = (currentIndex + 1) % enumValues.Length;
        
        return enumValues[nextIndex];
    }

    private void UpdateChangeDirectionTime() 
    {
        _changeDirectionTime = Time.time + _changeDirectionInterval;
    }

    private void ConfigureFromFile()
    {
        string[] props = { "Force" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        Force = propsFromConfig["Force"];
    }
}
