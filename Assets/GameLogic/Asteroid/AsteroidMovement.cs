using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _velocityMagnitude;
    [SerializeField]
    private float _maxRotation;

    private float _constantRotation;
    private Vector2 _direction;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetRandomRotationByScale();
        SetVelocity();
    }

    private void SetVelocity()
    {
        _rigidBody.velocity = _direction * _velocityMagnitude;
    }

    private void SetRandomRotationByScale()
    {
        _constantRotation = Random.Range(-_maxRotation / transform.localScale.x, _maxRotation / transform.localScale.x);
    }

    void Update()
    {
        transform.Rotate(0, 0, _constantRotation * Time.deltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.BACKGROUND.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

    // TODO (dev): Add knockback from other asteroids





    //private void AstroidExplotion(Collider2D other)
    //{
    //    if (transform.localScale.x > _explodeToSmallerAstroidsScaleTH)
    //        ExplodeToSmallerAstroids();
    //    Destroy(gameObject);
    //}

    //private void ExplodeToSmallerAstroids()
    //{
    //    int numOfSmallerAstroids = Random.Range(2, 4);

    //    for (int i = 0; i < numOfSmallerAstroids; i++)
    //    {
    //        GameObject smallerAstroid = Instantiate(
    //            _astroidPrefab, 
    //            new Vector3(transform.position.x + Random.Range(-0.1f, 0.1f), transform.position.y + Random.Range(-0.1f, 0.1f), 0), 
    //            Quaternion.AngleAxis(0, Vector3.forward));

    //        smallerAstroid.SendMessage("SetScale", transform.localScale.x / 2);
    //        smallerAstroid.SendMessage("Set360Velocity");
    //    }
    //}
}