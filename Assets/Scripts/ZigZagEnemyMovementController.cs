using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZigZagEnemyMovementController : MonoBehaviour
{

    public float EnemyThrust, SpeedLimit, KnockbackGunMultiplier;
    public bool UseConfigFile;
    public int ItemDropRate, _directionZigzag;
    private Rigidbody2D _rigidBody;
    private float _changeDirectionInterval, 
        _changeDirectionTime, _velocity;
    private Vector2 _direction;
    public GameObject ItemPrefab;


    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        if (UseConfigFile)
            ConfigureFromFile();

        _directionZigzag = 1;
        _changeDirectionInterval = 1;
        _velocity = 2;

        _direction = GetRandomVector2OnCircle(1);
        _changeDirectionTime = Time.time + _changeDirectionInterval;

        tag = "ZigZagEnemy";

    }

    void Update()
    {
        if (DidZigZagTimePass()) 
            ChangeDirection();

        _rigidBody.velocity = _direction * _velocity;
    }

    private Vector2 GetRandomVector2OnCircle(float radius) 
    {
        float angle = Random.Range(0, 2 * Mathf.PI);
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        return new Vector2(x, y);
    }

    private Vector2 GetPlus90Degrees(Vector2 vector)
    {
        return new Vector2(-vector.y, vector.x);
    }

    private Vector2 GetMinus90Degrees(Vector2 vector)
    {
        return new Vector2(vector.y, -vector.x);
    }

    private void ChangeDirection()
    {
        if (_directionZigzag == 1)
        {
            _direction = GetPlus90Degrees(_direction);
            _directionZigzag--;
        }
        else 
        {
            _direction = GetMinus90Degrees(_direction);
            _directionZigzag++;
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
        if (_directionZigzag == 1)
        {
            GetMinus90Degrees(_direction);
            GetMinus90Degrees(_direction);
        }
        else
        {
            GetPlus90Degrees(_direction);
            GetPlus90Degrees(_direction);
        }
    }

    private void KnockMeBack(Collider2D other, float otherThrust = 1f)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust * otherThrust), ForceMode2D.Impulse);
    }

    private void ConfigureFromFile()
    {
        string[] props = { "SpeedLimit", "EnemyThrust", "KnockbackGunMultiplier", "ItemDropRate" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        SpeedLimit = propsFromConfig["SpeedLimit"];
        EnemyThrust = propsFromConfig["EnemyThrust"];
        KnockbackGunMultiplier = propsFromConfig["KnockbackGunMultiplier"];
        ItemDropRate = (int)propsFromConfig["ItemDropRate"];
    }
}
