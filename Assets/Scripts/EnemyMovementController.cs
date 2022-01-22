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
    public float SpeedLimit;
    private Rigidbody2D _rigidBody;
    private float _xForce;
    private float _yForce;
    
    void Start()
    {
        if (UseConfigFile)
        {
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
            string[] props = { "MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed", "EnemyThrust", "SpeedLimit", "_rigidBody.drag"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
            EnemyThrust = propsFromConfig["EnemyThrust"];
            SpeedLimit = propsFromConfig["SpeedLimit"];
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
        Debug.Log("ANOTHER DIRECTION");      
        _xForce = GoInAnotherXDirection();
        _yForce = GoInAnotherYDirection();
        _rigidBody.velocity = new Vector2(0f, 0f);  
        Debug.Log("ANOTHER X DIRECTION" + _xForce + "ANOTHER Y DIRECTION" + _yForce);      
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

    private void KnockBack(Collider2D other)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust), ForceMode2D.Impulse);
    }

    private bool IsUnderSpeedLimit()
    {
        return Utils.VectorToAbsoluteValue(_rigidBody.velocity) < SpeedLimit;
    }
}
