using Assets.Common;
using System.Collections.Generic;
using UnityEngine;


public class AstroidtMovementController : MonoBehaviour
{

    public GameObject AstroidPrefab;
    public bool UseConfigFile;
    public float MaxXSpeed, MaxYSpeed, MinRotation, MaxRotation, ExplodeToSmallerAstroidsScaleTH;
    private float _rotationSpeed;


    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();

        GetComponent<Rigidbody2D>().velocity = GetVelocity(MaxXSpeed, MaxYSpeed);
        _rotationSpeed = GetRotationSpeed(MaxRotation* (-1), MaxRotation);
    }

    void SetScale (int scale) 
    {
        transform.localScale = new Vector3(scale, scale, 1);
    }

    void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        transform.Rotate(0, 0, _rotationSpeed, Space.Self);
    }   

    private float GetRotationSpeed(float minRotation, float maxRotation)
    {
        return UnityEngine.Random.Range(minRotation / transform.localScale.x, maxRotation / transform.localScale.x);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            AstroidExplotion();
    }

    private void AstroidExplotion()
    {
        if(transform.localScale.x > ExplodeToSmallerAstroidsScaleTH)
            ExplodeToSmallerAstroids();
        Destroy(gameObject);   
    }

    private void ExplodeToSmallerAstroids()
    {
        GameObject smallerAstroid;
        for(int i=0; i<Random.Range(2,4); i++)
        {
            smallerAstroid = Instantiate(AstroidPrefab,  new Vector3(transform.position.x, transform.position.y, 0), Quaternion.AngleAxis(0, Vector3.forward));
            smallerAstroid.SendMessage("SetScale", transform.localScale.x/2);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("OutOfScreen"))
            Destroy(gameObject);
    }

    Vector2 GetVelocity(float maxXSpeed, float maxYSpeed)
    {
        return new Vector2(GetXVelocity(maxXSpeed), GetYVelocity(maxYSpeed) );
    }

    private float GetXVelocity(float maxXSpeed) 
    {
        if (transform.position.x > 0)
            return UnityEngine.Random.Range(maxXSpeed / transform.localScale.x * (-1), 0f);
        else 
            return UnityEngine.Random.Range(0f, maxXSpeed / transform.localScale.x);
    }

    private float GetYVelocity(float maxYSpeed) 
    {
        if (transform.position.y > 0)
            return UnityEngine.Random.Range(maxYSpeed / transform.localScale.x * (-1), 0f);
        else
            return UnityEngine.Random.Range(0f, maxYSpeed / transform.localScale.x);
    }

    private void ConfigureFromFile()
    {
        string[] props = {"MaxXSpeed", "MaxYSpeed", "MinRotation", "MaxRotation", "ExplodeToSmallerAstroidsScaleTH"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        MaxXSpeed = propsFromConfig["MaxXSpeed"];
        MaxYSpeed = propsFromConfig["MaxYSpeed"];
        MinRotation = propsFromConfig["MinRotation"];
        MaxRotation = propsFromConfig["MaxRotation"];
        ExplodeToSmallerAstroidsScaleTH =  propsFromConfig["ExplodeToSmallerAstroidsScaleTH"];
    }

}