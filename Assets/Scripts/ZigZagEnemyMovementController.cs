using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZigZagEnemyMovementController : MonoBehaviour
{

    public float EnemyThrust, SpeedLimit, KnockbackGunMultiplier, Velocity;
    public bool UseConfigFile;
    public int ItemDropRate;
    public GameObject ItemPrefab;

    private Rigidbody2D _rigidBody;
    private float _changeDirectionInterval, _changeDirectionTime;
    private Vector2 _direction;
    private float _zigZagAngle;
    private int _directionZigzagFlag;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        if (UseConfigFile)
            ConfigureFromFile();

        _directionZigzagFlag = 1;
        _changeDirectionInterval = 1.5f;
        _zigZagAngle = 60;
        _direction = GetRandomVector2OnCircle(1);

        _changeDirectionTime = Time.time + _changeDirectionInterval;
        tag = "ZigZagEnemy";

    }

    void Update()
    {
        if (DidZigZagTimePass()) 
            ChangeDirection();

        _rigidBody.velocity = _direction * Velocity;
    }

    private Vector2 GetRandomVector2OnCircle(float radius) 
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        return new Vector2(x, y);
    }

    // TODO: move to a utility class
    Vector2 AddAngleToVector2(Vector2 vector, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        float currentAngle = Mathf.Atan2(vector.y, vector.x);
        float newAngle = currentAngle + angleInRadians;
        float x = Mathf.Cos(newAngle);
        float y = Mathf.Sin(newAngle);
        return new Vector2(x, y);
    }

    private void ChangeDirection()
    {
        if (_directionZigzagFlag == 1)
        {
            _direction = AddAngleToVector2(_direction, _zigZagAngle);
            _directionZigzagFlag--;
        }
        else 
        {
            _direction = AddAngleToVector2(_direction, _zigZagAngle * (-1));
            _directionZigzagFlag++;
        }
        UpdateChangeDirectionTime();
    }

    private bool DidZigZagTimePass()
    {
        return Time.time > _changeDirectionTime;
    }

    private void UpdateChangeDirectionTime()
    {
        _changeDirectionTime = Time.time + _changeDirectionInterval;
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
        AddAngleToVector2(_direction, 180);
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
