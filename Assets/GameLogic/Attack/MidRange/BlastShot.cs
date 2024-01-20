using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class BlastShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private GameObject blastPrefab;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _rigidBody.velocity = transform.up * _speed;
    }

    private void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            InitiateBlast();
        }
    }

    private void InitiateBlast()
    {
        GameObject blast = Instantiate(blastPrefab, transform.position, transform.rotation, transform.parent);
        blast.GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
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