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
    public float EnemyThrust;
    public bool UseConfigFile;
    private float _xForce;
    private float _yForce;
    private Rigidbody2D _rigidBody;
    public float speedLimit;

    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
        {
            Debug.Log("BB");
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
        //_rigidBody.velocity = new Vector2(_xSpeed, _ySpeed);
        if(IsUnderSpeedLimit())
            _rigidBody.AddForce(new Vector2(_xForce, _yForce));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            Destroy(gameObject);
        else if (other.gameObject.CompareTag("Halalit"))
            KnockBack(other);
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, otherCollider2D.GetComponent<Rigidbody2D>(), EnemyThrust), ForceMode2D.Impulse);
    }

    private bool IsUnderSpeedLimit()
    {
        return Utils.VectorToAbsoluteValue(_rigidBody.velocity) < speedLimit;
    }
}
