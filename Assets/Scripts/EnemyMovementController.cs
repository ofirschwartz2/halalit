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
    private float _xSpeed;
    private float _ySpeed;
    private Rigidbody2D _rigidBody;

    
    void Start()
    {
        if (UseConfigFile)
        {
            Debug.Log("BB");
            string[] props = { "MinXSpeed", "MaxXSpeed", "MinYSpeed", "MaxYSpeed", "EnemyThrust"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
            MinXSpeed = propsFromConfig["MinXSpeed"];
            MaxXSpeed = propsFromConfig["MaxXSpeed"];
            MinYSpeed = propsFromConfig["MinYSpeed"];
            MaxYSpeed = propsFromConfig["MaxYSpeed"];
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
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * normalizedSpeed(otherCollider2D), ForceMode2D.Impulse);
    }

    private float normalizedSpeed(Collider2D otherCollider2D)
    {
        return (Utils.VectorToAbsoluteValue(otherCollider2D.GetComponent<Rigidbody2D>().velocity) + Utils.VectorToAbsoluteValue(_rigidBody.velocity)) * EnemyThrust;
    }
}
