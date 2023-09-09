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
    private float _maxRotation;

    private float _constantRotation;

    public void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _constantRotation = Random.Range(-_maxRotation, _maxRotation);
    }

    void Update()
    {
        transform.Rotate(0, 0, _constantRotation * Time.deltaTime);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}
