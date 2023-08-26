using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class KnockbackShot : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _growthRate;

    private float _endOfLifeTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _endOfLifeTime = Time.time + _lifetime;
        _rigidBody.velocity = transform.up * _speed;
    }

    void FixedUpdate()
    {
        Grow();
        TryDie();
    }

    private void Grow()
    {
        transform.localScale *= _growthRate;
    }

    private void TryDie()
    {
        if (ShouldDie())
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldDie()
    {
        return Time.time >= _endOfLifeTime;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.EXTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
    }
}