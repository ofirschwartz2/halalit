using System;
using System.Collections.Generic;
using Assets.Common;
using Assets.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZigZagEnemyMovementController : MonoBehaviour
{

    public float EnemyThrust, SpeedLimit, KnockbackGunMultiplier, Velocity;
    public bool UseConfigFile;
    public int ItemDropRate;
    public GameObject ItemPrefab;

    private Rigidbody2D _rigidBody;
    private float _changeZigZagDirectionInterval, _changeZigZagDirectionTime, _changeFromDirectionAngle;
    private Vector2 _direction;
    private ZigZagEnemyDirection _zigZagDirectionFlag;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        if (UseConfigFile)
            ConfigureFromFile();

        _zigZagDirectionFlag = ZigZagEnemyDirection.ZAG;
        _changeZigZagDirectionInterval = 2;
        _changeFromDirectionAngle = 60;
        _direction = GetRandomVector2OnCircle();
        UpdateChangeZigZagDirectionTime();

        tag = "ZigZagEnemy";

    }

    void Update()
    {
        if (DidZigZagTimePass()) 
        {
            ChangeZigZagDirection();
        }

        Move();
    }

    private void Move()
    {
        if (IsUnderSpeedLimit()) 
        {
            _rigidBody.AddForce(_direction * Velocity);
        }
    }

    private bool IsUnderSpeedLimit()
    {
        return _rigidBody.velocity.magnitude < SpeedLimit;
    }

    private Vector2 GetRandomVector2OnCircle(float radius = 1) 
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        return new Vector2(x, y);
    }

    Vector2 AddAngleZigZagVector(Vector2 vector, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        float currentAngle = Mathf.Atan2(vector.y, vector.x);
        float newAngle = currentAngle + angleInRadians;
        float x = Mathf.Cos(newAngle);
        float y = Mathf.Sin(newAngle);
        return new Vector2(x, y);
    }

    private void ChangeZigZagDirection()
    {
        if (_zigZagDirectionFlag == ZigZagEnemyDirection.ZIG)
        {
            _direction = AddAngleZigZagVector(_direction, _changeFromDirectionAngle);
            _zigZagDirectionFlag = ZigZagEnemyDirection.ZAG;
        }
        else
        {
            _direction = AddAngleZigZagVector(_direction, _changeFromDirectionAngle * (-1));
            _zigZagDirectionFlag = ZigZagEnemyDirection.ZIG;
        }
        UpdateChangeZigZagDirectionTime();
    }

    private bool DidZigZagTimePass()
    {
        return Time.time > _changeZigZagDirectionTime;
    }

    private void UpdateChangeZigZagDirectionTime()
    {
        _changeZigZagDirectionTime = Time.time + _changeZigZagDirectionInterval;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            KillMe(other);
        else if (other.gameObject.CompareTag("Halalit") || other.gameObject.CompareTag("Astroid") || other.gameObject.CompareTag("Enemy"))
            KnockMeBack(other);
        else if (other.gameObject.CompareTag("KnockbackShot"))
            KnockMeBack(other, KnockbackGunMultiplier);
        else if (other.gameObject.CompareTag("Background"))
            GoInAnotherDirection();
    }

    private void KillMe(Collider2D other)
    {
        if (ShouldDropItem())
        {
            Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

            GameObject item = Instantiate(ItemPrefab, _rigidBody.transform.position, Quaternion.AngleAxis(0, Vector3.forward));
            item.SendMessage("StartFromInstantiation", normalizedDifference);
        }

        Destroy(gameObject);
    }

    private bool ShouldDropItem()
    {
        return Random.Range(0, 100) < ItemDropRate;
    }

    private void GoInAnotherDirection()
    {
        AddAngleZigZagVector(_direction, 180);
    }

    private void KnockMeBack(Collider2D other, float otherThrust = 1f)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust * otherThrust), ForceMode2D.Impulse);
    }

    private void ConfigureFromFile()
    {
        string[] props = { "SpeedLimit", "EnemyThrust", "KnockbackGunMultiplier", "ItemDropRate", "Velocity" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        SpeedLimit = propsFromConfig["SpeedLimit"];
        EnemyThrust = propsFromConfig["EnemyThrust"];
        KnockbackGunMultiplier = propsFromConfig["KnockbackGunMultiplier"];
        ItemDropRate = (int)propsFromConfig["ItemDropRate"];
        Velocity = propsFromConfig["Velocity"];
    }
}
