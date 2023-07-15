using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class BallShot : MonoBehaviour // TODO (refactor): the stats of a shot (when it collide with enemy) needs to be on the shot script
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _rigidBody.velocity = transform.right * _speed;
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag(Tag.SCREEN_EDGE.GetDescription()) && !other.CompareTag(Tag.SHOT.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}