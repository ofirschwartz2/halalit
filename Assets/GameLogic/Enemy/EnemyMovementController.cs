using System.Collections.Generic;
using Assets.Utils;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour 
{
    public float MinXForce, MaxXForce, MinYForce, MaxYForce, EnemyThrust, SpeedLimit, KnockbackGunMultiplier;
    public bool UseConfigFile;
    public int ItemDropRate;
    public GameObject ItemPrefab;

    private Rigidbody2D _rigidBody;
    private float _xForce, _yForce;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
            ConfigureFromFile();
            
        _xForce = Random.Range(MinXForce, MaxXForce);
        _yForce = Random.Range(MinYForce, MaxYForce);
        tag = "Enemy";
    }

    void Update()
    {
        if(IsUnderSpeedLimit())
            _rigidBody.AddForce(new Vector2(_xForce, _yForce));
    }

    void InnerOnTriggerEnter2D(Collider2D other)
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

    void InnerOnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("OutOfScreen"))
            Destroy(gameObject);
    }

    private void KillMe(Collider2D other)
    {
        if(ShouldDropItem())
        {
            Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

            GameObject item = Instantiate(ItemPrefab,  _rigidBody.transform.position, Quaternion.AngleAxis(0, Vector3.forward));
            item.SendMessage("StartFromInstantiation", normalizedDifference);
        }

        Destroy(gameObject);
    }
    
    private bool ShouldDropItem()
    {
        return Random.Range(0,100) < ItemDropRate;
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

    private void KnockMeBack(Collider2D other, float otherThrust = 1f)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust * otherThrust), ForceMode2D.Impulse);
    }
    
    private bool IsUnderSpeedLimit()
    {
        return Utils.GetVectorMagnitude(_rigidBody.velocity) < SpeedLimit;
    }

    private void ConfigureFromFile()
    {
        string[] props = { "MinXForce", "MaxXForce", "MinYForce", "MaxYForce", "EnemyThrust", "SpeedLimit", "_rigidBody.drag", "ItemDropRate", "KnockbackGunMultiplier"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        
        MinXForce = propsFromConfig["MinXForce"];
        MaxXForce = propsFromConfig["MaxXForce"];
        MinYForce = propsFromConfig["MinYForce"];
        MaxYForce = propsFromConfig["MaxYForce"];
        EnemyThrust = propsFromConfig["EnemyThrust"];
        SpeedLimit = propsFromConfig["SpeedLimit"];
        _rigidBody.drag = propsFromConfig["_rigidBody.drag"];
        ItemDropRate = (int)propsFromConfig["ItemDropRate"];
        KnockbackGunMultiplier = propsFromConfig["KnockbackGunMultiplier"];
    }
}