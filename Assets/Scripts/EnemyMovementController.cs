using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public float MinXSpeed;
    public float MaxXSpeed;
    public float MinYSpeed;
    public float MaxYSpeed;
    public float XofEndOfMap;
    public float YofEndOfMap;
    public float EnemyThrust;
    public bool UseConfigFile;
    private Rigidbody2D _rigidBody;
    private float _xSpeed;
    private float _ySpeed;
    
    void Start()
    {
        Debug.Log("AA");
        if (UseConfigFile)
        {
            Debug.Log("BB");
            string[] props = { "MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed", "XofEndOfMap", "YofEndOfMap", "EnemyThrust"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
            XofEndOfMap = propsFromConfig["XofEndOfMap"];
            YofEndOfMap = propsFromConfig["YofEndOfMap"];
            EnemyThrust = propsFromConfig["EnemyThrust"];
        }
        _rigidBody = GetComponent<Rigidbody2D>(); 
        _xSpeed = Random.Range(MinXSpeed, MaxXSpeed);
        _ySpeed = Random.Range(MinYSpeed, MaxYSpeed);
    }

    void Update()
    {
        _rigidBody.velocity = new Vector2(_xSpeed, _ySpeed);
        tag = "Enemy";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            Destroy(gameObject);
        else if (other.gameObject.CompareTag("Halalit"))
            KnockBack(other);
        else if (other.gameObject.CompareTag("Background"))
            GoInAnotherDirection();
    }

    private void GoInAnotherDirection()
    {
        _xSpeed = GoInAnotherXDirection();
        _ySpeed = GoInAnotherYDirection();
        _rigidBody.velocity = new Vector2(0f, 0f);        
    }

    private float GoInAnotherXDirection() 
    {
        if (_rigidBody.transform.position.x > XofEndOfMap)
            return Random.Range(MinXSpeed, 0f);
        else if (_rigidBody.transform.position.x < (-1) * XofEndOfMap)
            return Random.Range(0f, MaxXSpeed);
        return Random.Range(MinXSpeed, MaxXSpeed);
    }

    private float GoInAnotherYDirection() 
    {
        if (_rigidBody.transform.position.y > YofEndOfMap)
            return Random.Range(MinYSpeed, 0f);
        else if (_rigidBody.transform.position.y < (-1) * YofEndOfMap)
            return Random.Range(0f, MaxYSpeed);
        return Random.Range(MinYSpeed, MaxYSpeed);
    }
    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * NormalizedSpeed(otherCollider2D), ForceMode2D.Impulse);

    }

    private float NormalizedSpeed(Collider2D otherCollider2D)
    {
        return (Utils.VectorToAbsoluteValue(otherCollider2D.GetComponent<Rigidbody2D>().velocity) + Utils.VectorToAbsoluteValue(_rigidBody.velocity)) * EnemyThrust;
    }
}
