using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BlastShot : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private GameObject blastPrefab;
    [SerializeField]
    private float _lifetime;

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

    private void Update()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            InitiateBlast();
        }
    }

    private void InitiateBlast()
    {
        Instantiate(blastPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription())) 
        {
            InitiateBlast();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription())) 
        {
            Destroy(gameObject);
        }
    }
}