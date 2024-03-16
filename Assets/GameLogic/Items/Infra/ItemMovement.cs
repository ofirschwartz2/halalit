using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Linq;
using log4net.Util;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ItemMovement : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D _rigidBody;
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _transparencyPeriod;
    [SerializeField]
    private float _grabbedOpacity;


    private float _itemLifeTime;
    private float _rotationSpeed;
    private bool _removedTransparencyPeriodDone;

    public void Start()
    {
        _removedTransparencyPeriodDone = false;
        _rotationSpeed = Random.Range(-_maxRotation, _maxRotation);
    }

    public void FixedUpdate()
    {
        _rigidBody.MoveRotation(_rigidBody.rotation + _rotationSpeed * Time.deltaTime);
        _itemLifeTime += Time.deltaTime;

        if (!_removedTransparencyPeriodDone)
        {
            TryRemoveTransparency();
        }
    }

    private void TryRemoveTransparency()
    {
        if (_itemLifeTime >= _transparencyPeriod && !IsOverlappingAstroidsOrEnemies())
        {
            transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(Assets.Enums.Layer.ITEM_COLLISIONS.GetDescription());
            _removedTransparencyPeriodDone = true;
        }
    }

    private bool IsOverlappingAstroidsOrEnemies()
    {
        return Physics2D.OverlapCircleAll(transform.position, transform.localScale.x)
                    .Where(collider => collider.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) ||
                                       collider.gameObject.CompareTag(Tag.ENEMY.GetDescription())).ToArray().Length > 0;
    }

    public void Grabbed(UnityEngine.Transform grabber)
    {
        _rigidBody.isKinematic = true;
        _rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = grabber.position;
        transform.SetParent(grabber);
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(Assets.Enums.Layer.ITEM_TRIGGERS.GetDescription());
        Utils.ChangeOpacity(GetComponent<Renderer>(), _grabbedOpacity);
    }

    public void UnGrabbed()
    {
        _rigidBody.isKinematic = false;
        _rigidBody.constraints = RigidbodyConstraints2D.None;
        transform.parent = null;
        transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(Assets.Enums.Layer.ITEM_COLLISIONS.GetDescription());
        Utils.ChangeOpacity(GetComponent<Renderer>(), 1);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}