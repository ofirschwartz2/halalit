using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Linq;
using log4net.Util;

public class ItemMovement : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D _rigidBody;
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _transparencyPeriod;

    private int _defaultLayer;
    private float _itemLifeTime;
    private float _rotationSpeed;

    public void Start()
    {
        _defaultLayer = gameObject.layer;
        _rotationSpeed = Random.Range(-_maxRotation, _maxRotation);
    }

    public void FixedUpdate()
    {
        _rigidBody.MoveRotation(_rigidBody.rotation + _rotationSpeed * Time.deltaTime);
        _itemLifeTime += Time.deltaTime;

        if (gameObject.layer == _defaultLayer && _itemLifeTime >= _transparencyPeriod)
        {
            TryRemoveTransparency();
        }
    }

    private void TryRemoveTransparency()
    {
        if (Physics2D.OverlapCircleAll(transform.position, transform.localScale.x)
            .Where(collider => collider.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) || 
                               collider.gameObject.CompareTag(Tag.ENEMY.GetDescription())).ToArray().Length == 0)
        {
            transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(Layer.ITEM_COLLISIONS.GetDescription());
        }
    }

    public void Grabbed(UnityEngine.Transform grabber)
    {
        _rigidBody.isKinematic = true;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = grabber.position;
        transform.SetParent(grabber);
    }

    public void UnGrabbed()
    {
        _rigidBody.isKinematic = false;
        _rigidBody.constraints = RigidbodyConstraints2D.None;
        transform.parent = null;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}