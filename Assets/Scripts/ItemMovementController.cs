using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common;

public class ItemMovementController : MonoBehaviour
{
    
    public float MaxSpeed;
    public bool UseConfigFile;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();
    }

    void SetVelocity(Vector2 normalized180Direction) 
    {
        GetComponent<Rigidbody2D>().velocity = normalized180Direction;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("OutOfScreen"))
            Destroy(gameObject);
    }

    private void ConfigureFromFile()
    {
        string[] props = {"MaxSpeed"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

        MaxSpeed = propsFromConfig["MaxSpeed"];
    }
}
