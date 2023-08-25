using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class AsteroidDestructor : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _singleAxisDestructionDistance;

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
        DestroyRogueAsteroids();
    }

    private void DestroyRogueAsteroids()
    {
        if (Mathf.Abs(transform.position.x - _creationPosition.x) > _singleAxisDestructionDistance)
        {
            Destroy(gameObject);
        }

        if (Mathf.Abs(transform.position.y - _creationPosition.y) > _singleAxisDestructionDistance)
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
        if (other.gameObject.CompareTag(Tag.SHOT.GetDescription()))
        {
            OnAsteroidDestruction();
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
    }

    private void OnAsteroidDestruction()
    {
        InvokeAsteroidDestructionEvent();
        InvokeItemDropEvent();
    }

    private void InvokeAsteroidDestructionEvent()
    {
        AsteroidEventArguments asteroidEventArguments = new(transform.position, _rigidBody.velocity, transform.localScale.x);
        AsteroidEvent.Invoke(EventName.ASTEROID_DESTRUCTION, this, asteroidEventArguments);
    }

    private void InvokeItemDropEvent()
    {
        RangeAttribute itemDropLuck = new(0, 40); // TODO (dev): make the luck random and multiplie by asteroid scale
        DropEventArguments dropEventArguments = new(DropType.ITEM_DROP, transform.position, Vector2.zero, itemDropLuck); 
        DropEvent.Invoke(EventName.ITEM_DROP, this, dropEventArguments);
    }
}


