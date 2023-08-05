using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class ItemMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _maxVelocity;
    [SerializeField]
    private float _maxRotation;

    private float _constantRotation;

    public void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _constantRotation = Random.Range(-_maxRotation, _maxRotation);

        SetVelocity();
    }

    private void SetVelocity()
    {
        _rigidBody.velocity = Utils.GetRandomVector(-_maxVelocity, _maxVelocity, -_maxVelocity, _maxVelocity);
    }

    void Update()
    {
        transform.Rotate(0, 0, _constantRotation * Time.deltaTime);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.EXTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}
