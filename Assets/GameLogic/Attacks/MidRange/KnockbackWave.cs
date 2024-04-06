using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
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

#if UNITY_EDITOR
    internal float GetLifetime()
    {
        return _lifetime;
    }

    internal float GetSpeed()
    {
        return _speed;
    }
#endif

}