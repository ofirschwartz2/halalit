using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

class KnockbackWave : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _growthRate;
    [SerializeField]
    private float _decreaseGrowthRate; // between 0-1
    [SerializeField]
    private float _maxLaunchScale = 2f;
    [SerializeField]
    private float _launchSpeedAtScaleOne = 12f;
    [SerializeField]
    private float _maxLaunchSpeed = 12f;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _rigidBody.velocity = transform.up * _speed;
    }

    void FixedUpdate()
    {
        Grow();
        DecreaseGrowthRate();
        TryDie();
    }

    private void Grow()
    {
        transform.localScale = new Vector3(transform.localScale.x * _growthRate, transform.localScale.y);
    }

    private void DecreaseGrowthRate()
    {
        _growthRate = 1 + (_growthRate - 1) * _decreaseGrowthRate;
    }

    private void TryDie()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldLaunch(other.gameObject))
        {
            Launch(other.gameObject);
        }
    }

    private void Launch(GameObject asteroid)
    {
        var scale = asteroid.transform.localScale.x;
        var movement = asteroid.GetComponent<KinematicMovement>();
        movement.SetDirection(transform.up);
        movement.SetSpeed(Mathf.Min(_launchSpeedAtScaleOne / scale, _maxLaunchSpeed));
    }

    #region Predicates
    private bool ShouldLaunch(GameObject other)
    {
        return other.CompareTag(Tag.ASTEROID.GetDescription()) && other.transform.localScale.x <= _maxLaunchScale;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetLifetime()
    {
        return _lifetime;
    }

    internal float GetSpeed()
    {
        return _speed;
    }

    internal float GetMaxLaunchScale()
    {
        return _maxLaunchScale;
    }

    internal float GetLaunchSpeedAtScaleOne()
    {
        return _launchSpeedAtScaleOne;
    }

    internal float GetMaxLaunchSpeed()
    {
        return _maxLaunchSpeed;
    }
#endif

}