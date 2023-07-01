using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class ZigZagEnemyMovementController : MonoBehaviour 
{
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        if (UseConfigFile)
            ConfigureFromFile();
            
        tag = "ZigZagEnemy";
    }

    void Update()
    {
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
    }

    private void KnockMeBack(Collider2D other, float otherThrust = 1f)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust * otherThrust), ForceMode2D.Impulse);
    }

    private void ConfigureFromFile()
    {
        string[] props = {};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
    }
}
