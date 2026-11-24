using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class BallShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _lifetime;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _rigidBody.velocity = transform.up * _speed;
    }

    private void FixedUpdate()
    {
        if (HasLifetime() && Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) || other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
            Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
    }

    #region Predicates
    private bool HasLifetime()
    {
        return _lifetime > 0;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetLifeTime()
    {
        return _lifetime;
    }
#endif
}