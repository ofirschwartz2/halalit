using Assets.Common;
using System.Collections.Generic;
using UnityEngine;

public class AstroidtMovementController : MonoBehaviour
{

    public GameObject AstroidPrefab, ItemPrefab;
    public bool UseConfigFile;
    public float MaxXSpeed, MaxYSpeed, MinRotation, MaxRotation, ExplodeToSmallerAstroidsScaleTH;
    public int ItemDropRate;

    private float _rotationSpeed;
    private Rigidbody2D _rigidBody;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.velocity = GetVelocityByQuarters();
        _rotationSpeed = GetRotationSpeed(-MaxRotation, MaxRotation);
        ItemDropRate = (int)Mathf.Ceil(ItemDropRate / transform.localScale.x);
    }

    void SetScaleAndVelocity(float[] scaleAndVelocity) 
    {
        SetScale(scaleAndVelocity[0]);
        //SetVelocity(scaleAndVelocity[1]);
    }

    private void SetScale(float scale) 
    {
        transform.localScale = new Vector3(scale, scale, 1);
        ItemDropRate = (int)Mathf.Ceil(ItemDropRate / scale);
    }

    private void SetVelocity(float get360VelocityFlag) 
    {
        if (get360VelocityFlag == 1)
            _rigidBody.velocity = Get360Velocity();
        else
            _rigidBody.velocity = GetVelocityByQuarters();
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

    private void AstroidExplotion(Collider2D other)
    {
        if (ShouldDropItem())
        {
            Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;

            GameObject item = Instantiate(ItemPrefab,  _rigidBody.transform.position, Quaternion.AngleAxis(0, Vector3.forward));
            item.SendMessage("StartFromInstantiation", normalizedDifference);
        }
            
        if(transform.localScale.x > ExplodeToSmallerAstroidsScaleTH)
            ExplodeToSmallerAstroids();
        Destroy(gameObject);   
    }

    private bool ShouldDropItem()
    {
        return Random.Range(0,100) < ItemDropRate ;
    }

    private void ExplodeToSmallerAstroids()
    {
        GameObject smallerAstroid;
        for(int i = 0; i < Random.Range(2,3); i++)
        {
            smallerAstroid = Instantiate(AstroidPrefab,  new Vector3(transform.position.x + Random.Range(-0.1f,0.1f), transform.position.y + Random.Range(-0.1f,0.1f), 0), Quaternion.AngleAxis(0, Vector3.forward));
            float[] argumentsArray = new float[] { transform.localScale.x / 2, 1f };
            smallerAstroid.SendMessage("SetScaleAndVelocity", argumentsArray);
        }
    }

    public void InnerOnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            AstroidExplotion(other);
    }

    void InnerOnTriggerExit2D(Collider2D other)
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
        return UnityEngine.Random.Range(-MaxXSpeed / transform.localScale.x, MaxXSpeed / transform.localScale.x);
    }

    private float GetY360Velocity()
    {
        return UnityEngine.Random.Range(-MaxYSpeed / transform.localScale.x, MaxYSpeed / transform.localScale.x);
    }

    private float GetXVelocityByQuarters() 
    {
        if (transform.position.x > 0)
            return UnityEngine.Random.Range(-MaxXSpeed / transform.localScale.x, 0f);
        else 
            return UnityEngine.Random.Range(0f, MaxXSpeed / transform.localScale.x);
    }

    private float GetYVelocityByQuarters() 
    {
        if (transform.position.y > 0)
            return UnityEngine.Random.Range(-MaxYSpeed / transform.localScale.x, 0f);
        else
            return UnityEngine.Random.Range(0f, MaxYSpeed / transform.localScale.x);
    }

    private void ConfigureFromFile()
    {
        string[] props = {"MaxXSpeed", "MaxYSpeed", "MinRotation", "MaxRotation", "ExplodeToSmallerAstroidsScaleTH", "ItemDropRate"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        MaxXSpeed = propsFromConfig["MaxXSpeed"];
        MaxYSpeed = propsFromConfig["MaxYSpeed"];
        MinRotation = propsFromConfig["MinRotation"];
        MaxRotation = propsFromConfig["MaxRotation"];
        ExplodeToSmallerAstroidsScaleTH =  propsFromConfig["ExplodeToSmallerAstroidsScaleTH"];
        ItemDropRate = (int)propsFromConfig["ItemDropRate"];
    }

}