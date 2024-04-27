using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class ShotgunShot : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _averageSpeed;
    [SerializeField]
    private float _speedVariance;
    [SerializeField]
    private AnimationCurve _movementCurve;
    [SerializeField]
    private float _averageLifeTime;
    [SerializeField]
    private float _lifeTimeVariance; 

    private float _lifeTime, _startOfLifeTime, _endOfLifeTime, _speed;

    void Start()
    {
        SetLifeTime();
        SetSpeed();
    }

    void FixedUpdate()
    {
        _rigidBody.velocity = transform.up * _speed * (1 - _movementCurve.Evaluate((Time.time - _startOfLifeTime) / _lifeTime));
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    private void SetSpeed()
    {
        _speed = Utils.GetRandomBetween(_averageSpeed - _speedVariance, _averageSpeed + _speedVariance);
    }

    private void SetLifeTime()
    {
        _lifeTime = SeedlessRandomGenerator.Range(_averageLifeTime - _lifeTimeVariance, _averageLifeTime + _lifeTimeVariance);
        _startOfLifeTime = Time.time;
        _endOfLifeTime = _startOfLifeTime + _lifeTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    internal float GetMaxLifeTime()
    {
        return _averageLifeTime + _lifeTimeVariance;
    }

    internal float GetMinLifeTime()
    {
        return _averageLifeTime - _lifeTimeVariance;
    }
#endif
}