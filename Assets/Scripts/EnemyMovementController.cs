using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public float MinXForce, MaxXForce, MinYForce, MaxYForce;
    public float EnemyThrust;
    public bool UseConfigFile;
    public float SpeedLimit;
    private Rigidbody2D _rigidBody;
    private float _xForce;
    private float _yForce;
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
        {
            string[] props = { "MinXForce", "MaxXForce", "MinYForce", "MaxYForce", "EnemyThrust", "SpeedLimit", "_rigidBody.drag"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXForce = propsFromConfig["MinXForce"];
            MaxXForce = propsFromConfig["MaxXForce"];
            MinYForce = propsFromConfig["MinYForce"];
            MaxYForce = propsFromConfig["MaxYForce"];
            EnemyThrust = propsFromConfig["EnemyThrust"];
            SpeedLimit = propsFromConfig["SpeedLimit"];
            _rigidBody.drag = propsFromConfig["_rigidBody.drag"];
        }
        _xForce = Random.Range(MinXForce, MaxXForce);
        _yForce = Random.Range(MinYForce, MaxYForce);
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
        else if (other.gameObject.CompareTag("Halalit") || other.gameObject.CompareTag("Astroid") || other.gameObject.CompareTag("Enemy"))
            KnockMeBack(other);
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
            return Random.Range(MinXForce, 0f);
        else if (_rigidBody.transform.position.x < 0)
            return Random.Range(0f, MaxXForce);
        return Random.Range(MinXForce, MaxXForce);
    }

    private float GoInAnotherYDirection() 
    {
        if (_rigidBody.transform.position.y > 0)
            return Random.Range(MinYForce, 0f);
        else if (_rigidBody.transform.position.y < 0)
            return Random.Range(0f, MaxYForce);
        return Random.Range(MinYForce, MaxYForce);
    }

    private void KnockMeBack(Collider2D other)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust), ForceMode2D.Impulse);
    }

    private bool IsUnderSpeedLimit()
    {
        return Utils.GetVectorMagnitude(_rigidBody.velocity) < SpeedLimit;
    }
}
