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
    private float _xForce;
    private float _yForce;
    public float speedLimit;
    
    void Start()
    {
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
        if (UseConfigFile)
        {
            string[] props = { "MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed", "EnemyThrust", "speedLimit", "_rigidBody.drag"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
            EnemyThrust = propsFromConfig["EnemyThrust"];
            speedLimit = propsFromConfig["speedLimit"];
            _rigidBody.drag = propsFromConfig["_rigidBody.drag"];
        }
        _xForce = Random.Range(MinXSpeed, MaxXSpeed);
        _yForce = Random.Range(MinYSpeed, MaxYSpeed);
        tag = "Enemy";
    }

    void Update()
    {
        if(IsUnderSpeedLimit())
            _rigidBody.AddForce(new Vector2(_xForce, _yForce));
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
        _xForce = GoInAnotherXDirection();
        _yForce = GoInAnotherYDirection();
        _rigidBody.velocity = new Vector2(0f, 0f);        
    }

    private float GoInAnotherXDirection() 
    {
        if (_rigidBody.transform.position.x > 0)
            return Random.Range(MinXSpeed, 0f);
        else if (_rigidBody.transform.position.x < 0)
            return Random.Range(0f, MaxXSpeed);
        return Random.Range(MinXSpeed, MaxXSpeed);
    }

    private float GoInAnotherYDirection() 
    {
        if (_rigidBody.transform.position.y > 0)
            return Random.Range(MinYSpeed, 0f);
        else if (_rigidBody.transform.position.y < 0)
            return Random.Range(0f, MaxYSpeed);
        return Random.Range(MinYSpeed, MaxYSpeed);
    }
    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, otherCollider2D.GetComponent<Rigidbody2D>(), EnemyThrust), ForceMode2D.Impulse);
    }

    private bool IsUnderSpeedLimit()
    {
        return Utils.GetVectorMagnitude(_rigidBody.velocity) < speedLimit;
    }
}
