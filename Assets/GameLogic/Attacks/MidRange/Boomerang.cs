using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsMode")]
#endif

public class Boomerang : MonoBehaviour 
{
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    Vector2[] bezierCurve6InnerPoints;
    [SerializeField]
    private float _fetchRadius = 0.8f;

    private const int FETCH_HITS_BUFFER_SIZE = 8;

    private Vector2 _startPosition, _rightDirection;
    private float _shootingPercentPassed; // Between 0 to 1
    private float _creationTime;
    private GameObject _halalit;
    private int _itemTriggersLayerMask;
    private string _valuableTag;
    private readonly Collider2D[] _fetchHits = new Collider2D[FETCH_HITS_BUFFER_SIZE];
    private bool _hasFetched;
    private GameObject _carriedValuable;
    private IGrabbable _carriedGrabbable;

    void Start()
    {
        _rightDirection = transform.right;
        _startPosition = transform.position;
        _halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        _creationTime = Time.time;
        _itemTriggersLayerMask = LayerMask.GetMask(Layer.ITEM_TRIGGERS.GetDescription());
        _valuableTag = Tag.VALUABLE.GetDescription();
    }

    void FixedUpdate()
    {
        _shootingPercentPassed += Time.deltaTime / _lifetime;
        Shooting();

        if (!_hasFetched)
        {
            TryFetchValuable();
        }
    }

    private void Shooting()
    {
        transform.Rotate(0, 0, _rotationSpeed);

        transform.position = Utils.GetPointOnBezierCurveOf8(
            _startPosition,
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[0], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[1], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[2], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[3], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[4], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[5], _rightDirection),
            _halalit.transform.position, 
            _shootingPercentPassed);
    }

    private void TryFetchValuable()
    {
        var nearestValuable = GetNearestValuableInFetchRadius();
        if (nearestValuable == null)
        {
            return;
        }

        var grabbable = nearestValuable.GetComponent<IGrabbable>();
        if (grabbable == null)
        {
            return;
        }

        grabbable.Grabbed(transform);
        _carriedValuable = nearestValuable;
        _carriedGrabbable = grabbable;
        _hasFetched = true;
    }

    private GameObject GetNearestValuableInFetchRadius()
    {
        var hitsCount = Physics2D.OverlapCircleNonAlloc(transform.position, _fetchRadius, _fetchHits, _itemTriggersLayerMask);
        GameObject nearestValuable = null;
        var nearestDistance = float.MaxValue;

        for (int i = 0; i < hitsCount; i++)
        {
            var hitRigidbody = _fetchHits[i].attachedRigidbody;
            if (hitRigidbody == null || !hitRigidbody.CompareTag(_valuableTag))
            {
                continue;
            }

            var distance = Vector2.Distance(transform.position, hitRigidbody.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestValuable = hitRigidbody.gameObject;
            }
        }

        return nearestValuable;
    }

    private void End(bool returnedToHalalit)
    {
        TryReleaseValuable(returnedToHalalit);
        Destroy(gameObject);
    }

    private void TryReleaseValuable(bool returnedToHalalit)
    {
        if (!IsCarryingValuable())
        {
            return;
        }

        _carriedGrabbable.UnGrabbed();

        if (returnedToHalalit)
        {
            _carriedValuable.transform.position = _halalit.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription())) 
        {
            End(false);
        }   
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HalalitHitOnReturn(other))
        {
            End(true);
        }
        else if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            End(false);
        }
    }

    private bool HalalitHitOnReturn(Collider2D other)
    {
        return other.gameObject.CompareTag(Tag.HALALIT.GetDescription()) && (Time.time > _creationTime + Constants.BOOMERANG_OUT_OF_HALALIT_TIME);
    }

    private bool IsCarryingValuable()
    {
        return _carriedValuable != null && _carriedValuable.transform.parent == transform;
    }

    #if UNITY_EDITOR
    internal float GetLifetime()
    {
        return _lifetime;
    }
    #endif
}