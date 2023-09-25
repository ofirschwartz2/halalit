using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class ItemMovement : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D _rigidBody;
    [SerializeField]
    private float _maxRotation;

    private float _rotationSpeed;

    public void Start()
    {
        _rotationSpeed = Random.Range(-_maxRotation, _maxRotation);
    }

    public void FixedUpdate()
    {
        _rigidBody.MoveRotation(_rigidBody.rotation + _rotationSpeed * Time.deltaTime);
    }

    public void Grabbed()
    {
        _rigidBody.isKinematic = true;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation; 
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}
