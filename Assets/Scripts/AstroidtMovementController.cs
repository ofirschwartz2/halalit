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

        GetComponent<Rigidbody2D>().velocity = GetVelocityByQuarters();
        _rotationSpeed = GetRotationSpeed(MaxRotation* (-1), MaxRotation);
    }

    void SetScale(int scale) 
    {
        transform.localScale = new Vector3(scale, scale, 1);
    }

    void SetVelocity(bool get360VelocityFlag) 
    {
        if (get360VelocityFlag)
            GetComponent<Rigidbody2D>().velocity = Get360Velocity();
        else
            GetComponent<Rigidbody2D>().velocity = GetVelocityByQuarters();

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
            smallerAstroid.SendMessage("SetVelocity", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("OutOfScreen"))
            Destroy(gameObject);
    }

    Vector2 Get360Velocity()
    {
        return new Vector2(GetX360Velocity(), GetY360Velocity());
    }

    Vector2 GetVelocityByQuarters()
    {
        return new Vector2(GetXVelocityByQuarters(), GetYVelocityByQuarters());
    }

    private float GetX360Velocity()
    {
        return UnityEngine.Random.Range(MaxXSpeed / transform.localScale.x * (-1), MaxXSpeed / transform.localScale.x);
    }

    private float GetY360Velocity()
    {
        return UnityEngine.Random.Range(MaxYSpeed / transform.localScale.x * (-1), MaxYSpeed / transform.localScale.x);
    }

    private float GetXVelocityByQuarters() 
    {
        if (transform.position.x > 0)
            return UnityEngine.Random.Range(MaxXSpeed / transform.localScale.x * (-1), 0f);
        else 
            return UnityEngine.Random.Range(0f, MaxXSpeed / transform.localScale.x);
    }

    private float GetYVelocityByQuarters() 
    {
        if (transform.position.y > 0)
            return UnityEngine.Random.Range(MaxYSpeed / transform.localScale.x * (-1), 0f);
        else
            return UnityEngine.Random.Range(0f, MaxYSpeed / transform.localScale.x);
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