using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class AsteroidDestructor : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _singleAxisDestractionDistance;

    private Vector2 _creationPosition;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _creationPosition = gameObject.transform.position;
    }

    void Update()
    {
        DestroyRogureAsteroids();
    }

    private void DestroyRogureAsteroids()
    {
        if (Mathf.Abs(transform.position.x - _creationPosition.x) > _singleAxisDestractionDistance)
        {
            Destroy(gameObject);
        }

        if (Mathf.Abs(transform.position.y - _creationPosition.y) > _singleAxisDestractionDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.BACKGROUND.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}


