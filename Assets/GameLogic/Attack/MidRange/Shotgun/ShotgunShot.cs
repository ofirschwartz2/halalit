using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
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
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

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
        _lifeTime = Utils.GetRandomBetween(_averageLifeTime - _lifeTimeVariance, _averageLifeTime + _lifeTimeVariance);
        _startOfLifeTime = Time.time;
        _endOfLifeTime = _startOfLifeTime + _lifeTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
            Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
    }
}